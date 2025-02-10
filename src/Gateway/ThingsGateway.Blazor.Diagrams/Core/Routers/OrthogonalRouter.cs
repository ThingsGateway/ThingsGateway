using ThingsGateway.Blazor.Diagrams.Core.Anchors;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Routers;

public class OrthogonalRouter : Router
{
    private readonly Router _fallbackRouter;
    private double _shapeMargin;
    private double _globalMargin;

    public OrthogonalRouter(double shapeMargin = 10d, double globalMargin = 50d, Router? fallbackRouter = null)
    {
        _shapeMargin = shapeMargin;
        _globalMargin = globalMargin;
        _fallbackRouter = fallbackRouter ?? new NormalRouter();
    }

    public override Point[] GetRoute(Diagram diagram, BaseLinkModel link)
    {
        if (!link.IsAttached)
            return _fallbackRouter.GetRoute(diagram, link);

        if (link.Source is not SinglePortAnchor spa1)
            return _fallbackRouter.GetRoute(diagram, link);

        if (link.Target is not SinglePortAnchor targetAnchor)
            return _fallbackRouter.GetRoute(diagram, link);

        var sourcePort = spa1.Port;
        if (targetAnchor == null || sourcePort.Parent.Size == null || targetAnchor.Port.Parent.Size == null)
            return _fallbackRouter.GetRoute(diagram, link);

        var targetPort = targetAnchor.Port;

        var shapeMargin = _shapeMargin;
        var globalBoundsMargin = _globalMargin;
        var spots = new HashSet<Point>();
        var verticals = new List<double>();
        var horizontals = new List<double>();
        var sideA = sourcePort.Alignment;
        var sideAVertical = IsVerticalSide(sideA);
        var sideB = targetPort.Alignment;
        var sideBVertical = IsVerticalSide(sideB);
        var originA = GetPortPositionBasedOnAlignment(sourcePort);
        var originB = GetPortPositionBasedOnAlignment(targetPort);
        var shapeA = sourcePort.Parent.GetBounds(includePorts: true)!;
        var shapeB = targetPort.Parent.GetBounds(includePorts: true)!;
        var inflatedA = shapeA.Inflate(shapeMargin, shapeMargin);
        var inflatedB = shapeB.Inflate(shapeMargin, shapeMargin);

        if (inflatedA.Intersects(inflatedB))
        {
            shapeMargin = 0;
            inflatedA = shapeA;
            inflatedB = shapeB;
        }

        // Curated bounds to stick to
        var bounds = inflatedA.Union(inflatedB).Inflate(globalBoundsMargin, globalBoundsMargin);

        // Add edges to rulers
        verticals.Add(inflatedA.Left);
        verticals.Add(inflatedA.Right);
        horizontals.Add(inflatedA.Top);
        horizontals.Add(inflatedA.Bottom);
        verticals.Add(inflatedB.Left);
        verticals.Add(inflatedB.Right);
        horizontals.Add(inflatedB.Top);
        horizontals.Add(inflatedB.Bottom);

        // Rulers at origins of shapes
        (sideAVertical ? verticals : horizontals).Add(sideAVertical ? originA.X : originA.Y);
        (sideBVertical ? verticals : horizontals).Add(sideBVertical ? originB.X : originB.Y);

        // Points of shape antennas
        spots.Add(GetOriginSpot(originA, sideA, shapeMargin));
        spots.Add(GetOriginSpot(originB, sideB, shapeMargin));

        // Sort rulers
        verticals.Sort();
        horizontals.Sort();

        // Create grid
        var grid = RulersToGrid(verticals, horizontals, bounds);
        var gridPoints = GridToSpots(grid, new[] { inflatedA, inflatedB });

        // Add to spots
        spots.UnionWith(gridPoints);

        var ys = spots.Select(p => p.Y).Distinct().ToList();
        var xs = spots.Select(p => p.X).Distinct().ToList();
        ys.Sort();
        xs.Sort();

        var nodes = spots.ToDictionary(p => p, p => new Node(p));

        for (var i = 0; i < ys.Count; i++)
        {
            for (var j = 0; j < xs.Count; j++)
            {
                var b = new Point(xs[j], ys[i]);
                if (!nodes.ContainsKey(b))
                    continue;

                if (j > 0)
                {
                    var a = new Point(xs[j - 1], ys[i]);

                    if (nodes.TryGetValue(a, out Node? value))
                    {
                        value.ConnectedTo.Add(nodes[b]);
                        nodes[b].ConnectedTo.Add(value);
                    }
                }

                if (i > 0)
                {
                    var a = new Point(xs[j], ys[i - 1]);

                    if (nodes.TryGetValue(a, out Node? value))
                    {
                        value.ConnectedTo.Add(nodes[b]);
                        nodes[b].ConnectedTo.Add(value);
                    }
                }
            }
        }

        var nodeA = nodes[GetOriginSpot(originA, sideA, shapeMargin)];
        var nodeB = nodes[GetOriginSpot(originB, sideB, shapeMargin)];
        var path = AStarPathfinder.GetPath(nodeA, nodeB);

        if (path.Count > 0)
        {
            return path.ToArray();
        }
        else
        {
            return _fallbackRouter.GetRoute(diagram, link);
        }
    }

    private static Grid RulersToGrid(List<double> verticals, List<double> horizontals, Rectangle bounds)
    {
        var result = new Grid();
        verticals.Sort();
        horizontals.Sort();

        var lastX = bounds.Left;
        var lastY = bounds.Top;
        var column = 0;
        var row = 0;

        foreach (var y in horizontals)
        {
            foreach (var x in verticals)
            {
                result.Set(row, column++, new Rectangle(lastX, lastY, x, y));
                lastX = x;
            }

            // Last cell of the row
            result.Set(row, column, new Rectangle(lastX, lastY, bounds.Right, y));
            lastX = bounds.Left;
            lastY = y;
            column = 0;
            row++;
        }

        lastX = bounds.Left;

        // Last fow of cells
        foreach (var x in verticals)
        {
            result.Set(row, column++, new Rectangle(lastX, lastY, x, bounds.Bottom));
            lastX = x;
        }

        // Last cell of last row
        result.Set(row, column, new Rectangle(lastX, lastY, bounds.Right, bounds.Bottom));
        return result;
    }

    private static HashSet<Point> GridToSpots(Grid grid, Rectangle[] obstacles)
    {
        bool IsInsideObstacles(Point p)
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle.ContainsPoint(p))
                    return true;
            }

            return false;
        }

        void AddIfNotInsideObstacles(HashSet<Point> list, Point p)
        {
            if (!IsInsideObstacles(p))
            {
                list.Add(p);
            }
        }

        var gridPoints = new HashSet<Point>();
        foreach (var (row, data) in grid.Data)
        {
            var firstRow = row == 0;
            var lastRow = row == grid.Rows - 1;

            foreach (var (col, r) in data)
            {
                var firstCol = col == 0;
                var lastCol = col == grid.Columns - 1;
                var nw = firstCol && firstRow;
                var ne = firstRow && lastCol;
                var se = lastRow && lastCol;
                var sw = lastRow && firstCol;

                if (nw || ne || se || sw)
                {
                    AddIfNotInsideObstacles(gridPoints, r.NorthWest);
                    AddIfNotInsideObstacles(gridPoints, r.NorthEast);
                    AddIfNotInsideObstacles(gridPoints, r.SouthWest);
                    AddIfNotInsideObstacles(gridPoints, r.SouthEast);
                }
                else if (firstRow)
                {
                    AddIfNotInsideObstacles(gridPoints, r.NorthWest);
                    AddIfNotInsideObstacles(gridPoints, r.North);
                    AddIfNotInsideObstacles(gridPoints, r.NorthEast);
                }
                else if (lastRow)
                {
                    AddIfNotInsideObstacles(gridPoints, r.SouthEast);
                    AddIfNotInsideObstacles(gridPoints, r.South);
                    AddIfNotInsideObstacles(gridPoints, r.SouthWest);
                }
                else if (firstCol)
                {
                    AddIfNotInsideObstacles(gridPoints, r.NorthWest);
                    AddIfNotInsideObstacles(gridPoints, r.West);
                    AddIfNotInsideObstacles(gridPoints, r.SouthWest);
                }
                else if (lastCol)
                {
                    AddIfNotInsideObstacles(gridPoints, r.NorthEast);
                    AddIfNotInsideObstacles(gridPoints, r.East);
                    AddIfNotInsideObstacles(gridPoints, r.SouthEast);
                }
                else
                {
                    AddIfNotInsideObstacles(gridPoints, r.NorthWest);
                    AddIfNotInsideObstacles(gridPoints, r.North);
                    AddIfNotInsideObstacles(gridPoints, r.NorthEast);
                    AddIfNotInsideObstacles(gridPoints, r.East);
                    AddIfNotInsideObstacles(gridPoints, r.SouthEast);
                    AddIfNotInsideObstacles(gridPoints, r.South);
                    AddIfNotInsideObstacles(gridPoints, r.SouthWest);
                    AddIfNotInsideObstacles(gridPoints, r.West);
                    AddIfNotInsideObstacles(gridPoints, r.Center);
                }
            }
        }

        // Reduce repeated points and filter out those who touch shapes
        return gridPoints;
    }

    private static bool IsVerticalSide(PortAlignment alignment)
        => alignment == PortAlignment.Top || alignment == PortAlignment.Bottom;

    private static Point GetOriginSpot(Point p, PortAlignment alignment, double shapeMargin)
    {
        return alignment switch
        {
            PortAlignment.Top => p.Add(0, -shapeMargin),
            PortAlignment.Right => p.Add(shapeMargin, 0),
            PortAlignment.Bottom => p.Add(0, shapeMargin),
            PortAlignment.Left => p.Add(-shapeMargin, 0),
            _ => throw new NotImplementedException()
        };
    }
}

class Grid
{
    public Grid()
    {
        Data = new Dictionary<double, Dictionary<double, Rectangle>>();
    }

    public Dictionary<double, Dictionary<double, Rectangle>> Data { get; }
    public double Rows { get; private set; }
    public double Columns { get; private set; }

    public void Set(double row, double column, Rectangle rectangle)
    {
        Rows = Math.Max(Rows, row + 1);
        Columns = Math.Max(Columns, column + 1);

        if (!Data.TryGetValue(row, out Dictionary<double, Rectangle>? value))
        {
            value = new Dictionary<double, Rectangle>();
            Data.Add(row, value);
        }

        value.Add(column, rectangle);
    }
}

static class AStarPathfinder
{
    public static IReadOnlyList<Point> GetPath(Node start, Node goal)
    {
        var frontier = new PriorityQueue<Node, double>();
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
                break;

            foreach (var next in current.ConnectedTo)
            {
                var newCost = current.Cost + 1.0;
                if (current.Parent != null && IsChangeOfDirection(current.Parent.Position, current.Position, next.Position))
                {
                    newCost *= newCost;
                    newCost *= newCost;
                }

                if (next.Cost == 0 || newCost < next.Cost)
                {
                    next.Cost = newCost;
                    var priority = newCost + Heuristic(next.Position, goal.Position);
                    frontier.Enqueue(next, priority);
                    next.Parent = current;
                }
            }
        }

        var result = new List<Point>();
        var c = goal.Parent;

        while (c != null && c != start)
        {
            result.Insert(0, c.Position);
            c = c.Parent;
        }

        if (c == start)
        {
            result = SimplifyPath(result);

            // In case of paths with one bend
            if (result.Count > 2)
            {
                if (AreOnSameLine(result[^2], result[^1], goal.Position))
                {
                    result.RemoveAt(result.Count - 1);
                }

                if (AreOnSameLine(start.Position, result[0], result[1]))
                {
                    result.RemoveAt(0);
                }
            }

            return result;
        }
        else
        {
            return Array.Empty<Point>();
        }
    }

    private static bool AreOnSameLine(Point prev, Point curr, Point next)
    {
        return (prev.X == curr.X && curr.X == next.X) || (prev.Y == curr.Y && curr.Y == next.Y);
    }

    private static List<Point> SimplifyPath(List<Point> path)
    {
        for (var i = path.Count - 2; i > 0; i--)
        {
            var prev = path[i + 1];
            var curr = path[i];
            var next = path[i - 1];

            if (AreOnSameLine(prev, curr, next))
            {
                path.RemoveAt(i);
            }
        }

        return path;
    }

    private static bool IsChangeOfDirection(Point a, Point b, Point c)
    {
        if (a.X == b.X && b.X != c.X)
            return true;

        if (a.Y == b.Y && b.Y != c.Y)
            return true;

        return false;
    }

    private static double Heuristic(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}

class Node
{
    public Node(Point position)
    {
        Position = position;
        ConnectedTo = new List<Node>();
    }

    public Point Position { get; }
    public List<Node> ConnectedTo { get; }

    public double Cost { get; internal set; }
    public Node? Parent { get; internal set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Node item)
            return false;

        return Position.Equals(item.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}
