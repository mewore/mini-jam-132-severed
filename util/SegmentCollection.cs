using System.Collections.Generic;
using Godot;

public class SegmentCollection
{
    private List<(float, float)> segments = new List<(float, float)>();
    public IReadOnlyList<(float, float)> Segments => segments;
    public int Count => segments.Count;

    public SegmentCollection() { }

    public SegmentCollection((float, float) initalSegment)
    {
        segments.Add(initalSegment);
    }

    public SegmentCollection Clone()
    {
        SegmentCollection other = new SegmentCollection();
        other.segments = new List<(float, float)>(segments);
        return other;
    }

    public void Add((float, float) toAdd) => Add(new (float, float)[] { toAdd });

    public void Add(IEnumerable<(float, float)> toAdd)
    {
        foreach (var segment in toAdd)
        {
            segments.Add(segment);
        }
        merge(segments);
    }

    private static void merge(List<(float, float)> segments)
    {
        var oldSegments = new List<(float, float)>(segments);
        oldSegments.Sort(((float, float) first, (float, float) second) => first.Item1.CompareTo(second.Item1));
        segments.Clear();
        foreach (var segment in oldSegments)
        {
            if (segments.Count > 0 && segments[segments.Count - 1].Item2 >= segment.Item1)
            {
                segments[segments.Count - 1] = (segments[segments.Count - 1].Item1, segment.Item2);
            }
            else
            {
                segments.Add(segment);
            }
        }
    }

    public (float, float)? GetExplosionAt(float at, float radius)
    {
        foreach (var segment in segments)
        {
            if (at >= segment.Item1 && at <= segment.Item2)
            {
                return (Mathf.Max(at - radius, segment.Item1), Mathf.Min(at + radius, segment.Item2));
            }
        }
        return null;
    }

    public List<(float, float)> Remove((float, float) toRemove)
    {
        var removed = new List<(float, float)>();
        var newSegments = new List<(float, float)>(segments.Count);
        foreach (var segment in segments)
        {
            if (segment.Item2 <= toRemove.Item1 || segment.Item1 >= toRemove.Item2)
            {
                // Not intersecting at all
                newSegments.Add(segment);
            }
            else if (segment.Item1 <= toRemove.Item1 && segment.Item2 >= toRemove.Item2)
            {
                // toRemove inside segment
                newSegments.Add((segment.Item1, toRemove.Item1));
                newSegments.Add((toRemove.Item2, segment.Item2));
                removed.Add(toRemove);
            }
            else if (segment.Item1 >= toRemove.Item1 && segment.Item2 <= toRemove.Item2)
            {
                // segment inside toRemove
                removed.Add(segment);
            }
            else if (segment.Item2 <= toRemove.Item2)
            {
                // toRemove intersecting only with the right part of segment
                newSegments.Add((segment.Item1, toRemove.Item1));
                removed.Add((toRemove.Item1, segment.Item2));
            }
            else if (segment.Item1 >= toRemove.Item1)
            {
                // toRemove intersecting only with the left part of segment
                newSegments.Add((toRemove.Item2, segment.Item2));
                removed.Add((segment.Item1, toRemove.Item2));
            }
            else if (OS.IsDebugBuild())
            {
                throw new System.Exception(System.String.Format("({0} -:- {1}) \\ ({2} -:- {3})",
                    segment.Item1, segment.Item2, toRemove.Item1, toRemove.Item2));
            }
        }
        segments = newSegments;
        return removed;
    }

    public void Clear() => segments.Clear();

    public bool Contains(float position)
    {
        foreach (var segment in segments)
        {
            if (position > segment.Item2)
            {
                return false;
            }
            if (position >= segment.Item1)
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        var result = new string[segments.Count];
        int index = 0;
        foreach (var segment in segments)
        {
            result[index++] = System.String.Format("({0} -:- {1})", segment.Item1, segment.Item2);
        }
        return System.String.Join(", ", result);
    }
}
