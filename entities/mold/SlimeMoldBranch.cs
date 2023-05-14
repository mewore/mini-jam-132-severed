using Godot;
using System;
using System.Collections.Generic;

public class SlimeMoldBranch : Line2D
{
    [Export]
    private string slimeMoldBranchScene = "res://entities/mold/SlimeMoldBranch.tscn";

    [Export]
    private float branchTimeMin = 1f;

    [Export]
    private float branchTimeMax = 5f;

    [Export]
    private float maxSpeed = 10f;

    [Export]
    private float forwardAcceleration = 5f;

    [Export]
    private float randomAcceleration = 10f;

    [Export]
    private float lineRepulsion = 50f;

    [Export(PropertyHint.Range, "0,180")]
    private float angleSnap = 15f;

    [Export]
    private float speedUp = 1f;

    private float initialSpeedUp;

    [Export]
    private int widthDecrement = 2;

    private float initialWidth;
    private float widthMultiplier = 1f;

    [Export]
    private float beatWidthMultiplier = 1.5f;

    [Export(PropertyHint.Range, "0,1")]
    private float widthDecay = 0.1f;

    [Export]
    private float beatSpeedMultiplier = 10.5f;

    [Export(PropertyHint.Range, "0,1")]
    private float speedDecay = 0.1f;

    [Export]
    private float vulnerability = 200f;

    private bool active = true;
    private bool frozen = false;

    private Vector2 targetPoint;
    private Vector2 velocity;
    private Vector2 realVelocity;

    private List<Vector2> currentPoints = new List<Vector2>(new Vector2[] { Vector2.Zero });

    [Export]
    private int raycastSamples = 5;

    [Export(PropertyHint.Range, "0,1")]
    private float visionRange = .5f;

    private RayCast2D rayCast;

    private readonly List<SlimeMoldBranch> children = new List<SlimeMoldBranch>();
    private SlimeMoldBranch parent;
    public SlimeMoldBranch Parent { set => parent = value; }

    private SegmentCollection livingSegments = new SegmentCollection((0, Mathf.Inf));
    private readonly List<Line2D> segmentLines = new List<Line2D>();

    private bool isDamaged = false;
    private bool segmentLinesNeedUpdate = false;
    private bool needsUpdateAfterGrowth = false;
    private readonly SegmentCollection explosions = new SegmentCollection();

    private Rect2 precalculatedBoundingRect;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (velocity == Vector2.Zero)
        {
            velocity = Vector2.Right.Rotated((GD.Randi() % 12) * Mathf.Pi / 6) * maxSpeed;
        }
        realVelocity = velocity;
        currentPoints.Add(realVelocity.Normalized());
        Points = currentPoints.ToArray();
        _on_DirectionChangeTimer_timeout();
        if (slimeMoldBranchScene != null)
        {
            GetNode<Timer>("BranchTimer").Start((float)GD.RandRange(branchTimeMin, branchTimeMax));
        }

        var beatTimers = GetTree().GetNodesInGroup("beat_timer");
        if (beatTimers.Count > 0)
        {
            (beatTimers[0] as Timer).Connect("timeout", this, nameof(_on_beat));
        }

        initialSpeedUp = speedUp;
        initialWidth = Width;

        rayCast = GetNode<RayCast2D>("RayCast2D");
    }

    public override void _Process(float delta)
    {
        if (frozen)
        {
            return;
        }
        if (Width != initialWidth)
        {
            Width = Mathf.Lerp(Width, initialWidth, widthDecay);
        }
        if (segmentLinesNeedUpdate)
        {
            updateSegmentLines();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (explosions.Count > 0)
        {
            var totalLength = getTotalLength();
            if (active && explosions.Segments[explosions.Count - 1].Item2 >= totalLength - Mathf.Epsilon)
            {
                active = false;
                GetNode<Timer>("BranchTimer").Stop();
            }
            foreach (var segment in explosions.Segments)
            {
                livingSegments.Remove(segment);
            }
            explosions.Clear();
            segmentLinesNeedUpdate = true;
            isDamaged = true;
            if (livingSegments.Count <= 0)
            {
                QueueFree();
            }
        }
        if (!active)
        {
            return;
        }
        if (speedUp != initialSpeedUp)
        {
            speedUp = Mathf.Lerp(speedUp, initialSpeedUp, speedDecay);
        }
        delta *= speedUp;
        realVelocity += (targetPoint - (Position + currentPoints[currentPoints.Count - 1])).Normalized() * randomAcceleration * delta;
        realVelocity += realVelocity.Normalized() * forwardAcceleration * delta;
        realVelocity = realVelocity.LimitLength(maxSpeed);
        realVelocity += getRepulsion() * delta;
        realVelocity = realVelocity.LimitLength(maxSpeed);
        if (Mathf.Abs(realVelocity.AngleTo(velocity)) > angleSnap * Mathf.Pi / 180)
        {
            velocity = realVelocity;
            currentPoints.Add(currentPoints[currentPoints.Count - 1]);
        }
        currentPoints[currentPoints.Count - 1] += velocity * delta;
        Points = currentPoints.ToArray();
        if (isDamaged)
        {
            segmentLinesNeedUpdate = true;
        }
    }

    private Vector2 getRepulsion()
    {
        rayCast.Position = currentPoints[currentPoints.Count - 1];
        float angleRange = visionRange * Mathf.Pi;
        float angle = raycastSamples == 1 ? 0f : (-angleRange / 2);
        float angleStep = angleRange / (raycastSamples - 1);
        float motionAngle = velocity.Angle();
        Vector2 totalRepulsion = Vector2.Zero;

        for (int sample = 0; sample < raycastSamples; sample++, angle += angleStep)
        {
            rayCast.Rotation = motionAngle + angle;
            rayCast.ForceRaycastUpdate();
            if (!rayCast.IsColliding())
            {
                continue;
            }
            Vector2 obstacleNormal = rayCast.GetCollisionNormal();
            Vector2 raycastNormalized = rayCast.CastTo.Normalized().Rotated(rayCast.Rotation);
            Vector2 reflected = raycastNormalized - 2 * raycastNormalized.Dot(obstacleNormal) * obstacleNormal;
            float closeness = 1f - rayCast.GetCollisionPoint().DistanceTo(rayCast.GlobalPosition) / rayCast.CastTo.Length();
            closeness *= closeness;
            totalRepulsion += reflected * closeness;
        }
        return totalRepulsion / raycastSamples * lineRepulsion;
    }

    public void _on_BranchTimer_timeout()
    {
        active = false;
        if (Width <= widthDecrement)
        {
            return;
        }
        var scene = GD.Load<PackedScene>(slimeMoldBranchScene);
        var firstMold = scene.Instance<SlimeMoldBranch>();
        var secondMold = scene.Instance<SlimeMoldBranch>();

        firstMold.Position = secondMold.Position = Position + Points[Points.Length - 1];
        firstMold.velocity = secondMold.velocity = velocity;
        firstMold.Width = secondMold.Width = initialWidth - widthDecrement;
        firstMold.parent = secondMold.parent = this;
        if (GD.Randf() > 0.5f)
        {
            firstMold.Width = initialWidth;
            secondMold.Width = initialWidth / 2;
        }
        GetParent().AddChild(firstMold);
        children.Add(firstMold);
        if (secondMold.Width > 1f)
        {
            GetParent().AddChild(secondMold);
            children.Add(secondMold);
        }
    }

    public void _on_DirectionChangeTimer_timeout()
    {
        targetPoint = new Vector2(GD.Randf() * 1024, GD.Randf() * 600);
    }

    private void _on_beat()
    {
        speedUp = initialSpeedUp * beatSpeedMultiplier;
        Width = initialWidth * beatWidthMultiplier;
    }

    public void TestIntersection(Vector2 firstPoint, Vector2 secondPoint, float success)
    {
        foreach (var child in children)
        {
            child.TestIntersection(firstPoint, secondPoint, success);
        }
        firstPoint = ToLocal(firstPoint);
        secondPoint = ToLocal(secondPoint);

        if (!lineIsInRectangle(firstPoint, secondPoint, getBoundingRectangle()))
        {
            return;
        }

        var damage = vulnerability * success;

        var totalLength = getTotalLength();

        float damageToParent = 0f;
        float damageToChildren = 0f;

        float position = 0f;
        for (int index = 1; index < Points.Length; index++)
        {
            position += Points[index - 1].DistanceTo(Points[index]);
            var intersection = Geometry.SegmentIntersectsSegment2d(
                firstPoint, secondPoint, Points[index - 1], Points[index]);
            if (intersection == null)
            {
                continue;
            }
            Vector2 intersectionPoint = (Vector2)intersection;
            float intersectionPos = position - intersectionPoint.DistanceTo(Points[index]);
            var potentialExplosion = livingSegments.GetExplosionAt(intersectionPos, damage);
            if (potentialExplosion == null)
            {
                continue;
            }
            var explosion = ((float, float))potentialExplosion;
            explosions.Add(explosion);
            if (explosion.Item1 < Mathf.Epsilon)
            {
                damageToParent = Mathf.Max(damageToParent, damage - intersectionPos);
            }
            if (explosion.Item2 > totalLength - Mathf.Epsilon)
            {
                damageToChildren = Mathf.Max(damageToChildren, damage - (totalLength - intersectionPos));
            }
        }

        if (damageToChildren > 0f)
        {
            foreach (var child in children)
            {
                child.takeDamageAtBeginning(damageToChildren);
            }
        }
        if (damageToParent > 0f && parent != null)
        {
            parent.takeDamageAtEnd(damageToParent);
        }
    }

    private void takeDamageAtBeginning(float damage)
    {
        var potentialExplosion = livingSegments.GetExplosionAt(0f, damage);
        if (potentialExplosion == null)
        {
            return;
        }
        var explosion = ((float, float))potentialExplosion;
        explosions.Add(explosion);

        var totalLength = getTotalLength();
        if (explosion.Item2 > totalLength - Mathf.Epsilon)
        {
            foreach (var child in children)
            {
                child.takeDamageAtBeginning(damage - totalLength);
            }
        }
    }

    private void takeDamageAtEnd(float damage)
    {
        var totalLength = getTotalLength();
        var potentialExplosion = livingSegments.GetExplosionAt(totalLength, damage);
        if (potentialExplosion == null)
        {
            return;
        }
        var explosion = ((float, float))potentialExplosion;
        explosions.Add(explosion);

        if (explosion.Item1 < Mathf.Epsilon && parent != null)
        {
            parent.takeDamageAtEnd(damage - totalLength);
        }
    }

    private float getTotalLength()
    {
        float totalLength = 0f;
        for (int index = 1; index < Points.Length; index++)
        {
            totalLength += Points[index - 1].DistanceTo(Points[index]);
        }
        return totalLength;
    }

    private void updateSegmentLines()
    {
        if (SelfModulate.a > 0)
        {
            SelfModulate = new Color();
        }
        while (segmentLines.Count < livingSegments.Count)
        {
            var line = new Line2D();
            line.Width = Width;
            line.DefaultColor = DefaultColor;
            line.JointMode = LineJointMode.Bevel;
            line.BeginCapMode = LineCapMode.Round;
            line.EndCapMode = LineCapMode.Round;
            AddChild(line);
            segmentLines.Add(line);
        }
        List<Vector2[]> pointsForSegments = getPointsForSegment();
        for (int index = 0; index < segmentLines.Count; index++)
        {
            if (index >= pointsForSegments.Count)
            {
                segmentLines[index].Visible = false;
                continue;
            }
            segmentLines[index].Visible = true;
            segmentLines[index].Width = Width;
            // On second thought, I don't want this (it makes it harder to calculate coverage at the end of the level)
            // segmentLines[index].BeginCapMode = segment.Item1 < Mathf.Epsilon ? LineCapMode.Round : LineCapMode.None;
            // segmentLines[index].EndCapMode = segment.Item2 == Mathf.Inf ? LineCapMode.Round : LineCapMode.None;
            segmentLines[index].Points = pointsForSegments[index];
        }
    }

    private List<Vector2[]> getPointsForSegment()
    {
        var result = new List<Vector2[]>();
        int livingSegmentIndex = 0;
        (float, float) livingSegment = livingSegments.Segments[0];

        (float, float) lineSegment = (0f, 0f);
        var newPoints = new List<Vector2>();
        for (int index = 1; index < Points.Length; index++)
        {
            var pointDifference = Points[index] - Points[index - 1];
            var length = pointDifference.Length();
            var lineDirection = pointDifference / length;
            lineSegment = (lineSegment.Item2, lineSegment.Item2 + length);
            while (livingSegment.Item2 < lineSegment.Item1)
            {
                livingSegmentIndex++;
                result.Add(newPoints.ToArray());
                newPoints.Clear();
                if (livingSegmentIndex >= livingSegments.Count)
                {
                    return result;
                }
                livingSegment = livingSegments.Segments[livingSegmentIndex];
            }
            if (lineSegment.Item2 < livingSegment.Item1 || lineSegment.Item1 > livingSegment.Item2)
            {
                continue;
            }
            else if (lineSegment.Item1 >= livingSegment.Item1 && lineSegment.Item2 <= livingSegment.Item2)
            {
                // lineSegment in livingSegment
                if (newPoints.Count <= 0 || newPoints[newPoints.Count - 1] != Points[index - 1])
                {
                    newPoints.Add(Points[index - 1]);
                }
                newPoints.Add(Points[index]);
            }
            else if (livingSegment.Item1 >= lineSegment.Item1 && livingSegment.Item2 <= lineSegment.Item2)
            {
                // livingSegment in lineSegment
                newPoints.Add(Points[index - 1] + lineDirection * (livingSegment.Item1 - lineSegment.Item1));
                newPoints.Add(Points[index - 1] + lineDirection * (livingSegment.Item2 - lineSegment.Item1));
            }
            else if (livingSegment.Item2 < lineSegment.Item2)
            {
                // livingSegment to the left of lineSegment
                if (newPoints.Count <= 0 || newPoints[newPoints.Count - 1] != Points[index - 1])
                {
                    newPoints.Add(Points[index - 1]);
                }
                newPoints.Add(Points[index - 1] + lineDirection * (livingSegment.Item2 - lineSegment.Item1));
            }
            else if (livingSegment.Item1 > lineSegment.Item1)
            {
                // livingSegment to the right of lineSegment
                newPoints.Add(Points[index - 1] + lineDirection * (livingSegment.Item1 - lineSegment.Item1));
                newPoints.Add(Points[index]);
            }
            else if (OS.IsDebugBuild())
            {
                throw new System.Exception(System.String.Format("({0} -:- {1}) \\ ({2} -:- {1})",
                    lineSegment.Item1, lineSegment.Item2, livingSegment.Item1, livingSegment.Item2));
            }
        }
        if (result.Count < livingSegments.Count)
        {
            result.Add(newPoints.ToArray());
        }
        return result;
    }

    private static bool lineIsInRectangle(Vector2 firstPoint, Vector2 secondPoint, Rect2 rectangle)
    {
        var topLeft = rectangle.Position;
        var bottomRight = topLeft + rectangle.Size;
        var bottomLeft = new Vector2(topLeft.x, bottomRight.y);
        var topRight = new Vector2(bottomRight.x, topLeft.y);
        return rectangle.HasPoint(firstPoint)
            || rectangle.HasPoint(secondPoint)
            || Geometry.SegmentIntersectsSegment2d(firstPoint, secondPoint, topLeft, topRight) != null
            || Geometry.SegmentIntersectsSegment2d(firstPoint, secondPoint, topRight, bottomRight) != null
            || Geometry.SegmentIntersectsSegment2d(firstPoint, secondPoint, bottomRight, bottomLeft) != null
            || Geometry.SegmentIntersectsSegment2d(firstPoint, secondPoint, bottomLeft, topLeft) != null;
    }

    public void Disable()
    {
        precalculatedBoundingRect = getBoundingRectangle();
        active = false;
        frozen = true;
        GetNode<Timer>("BranchTimer").Stop();
        foreach (var child in children)
        {
            child.Disable();
        }
    }

    public void SetWidthMultiplier(float widthMultiplier)
    {
        this.widthMultiplier = widthMultiplier;
        Width = Mathf.Max(Width, initialWidth * widthMultiplier);
        foreach (var line in segmentLines)
        {
            line.Width = Width;
        }
        foreach (var child in children)
        {
            child.SetWidthMultiplier(widthMultiplier);
        }
    }

    public bool Contains(Vector2 point)
    {
        foreach (var child in children)
        {
            if (child.Contains(point))
            {
                return true;
            }
        }
        int livingSegmentIndex = 0;
        (float, float) livingSegment = (0f, Mathf.Inf);
        if (isDamaged)
        {
            if (livingSegments.Count <= 0)
            {
                return false;
            }
            livingSegment = livingSegments.Segments[0];
        }

        point = ToLocal(point);
        var boundingRect = getBoundingRectangle();
        var padding = new Vector2(Width, Width);
        boundingRect = boundingRect.Expand(boundingRect.Position - padding);
        boundingRect = boundingRect.Expand(boundingRect.Position + boundingRect.Size + padding);
        if (!boundingRect.HasPoint(point))
        {
            return false;
        }
        (float, float) lineSegment = (0f, 0f);
        for (int index = 1; index < Points.Length; index++)
        {
            var pointDifference = Points[index] - Points[index - 1];
            var length = pointDifference.Length();
            var lineDirection = pointDifference / length;
            lineSegment = (lineSegment.Item2, lineSegment.Item2 + length);
            while (livingSegment.Item2 < lineSegment.Item1)
            {
                livingSegmentIndex++;
                if (livingSegmentIndex >= livingSegments.Count)
                {
                    return false;
                }
                livingSegment = livingSegments.Segments[livingSegmentIndex];
            }
            if (lineSegment.Item2 < livingSegment.Item1)
            {
                continue;
            }
            else if (lineSegment.Item1 >= livingSegment.Item1 && lineSegment.Item2 <= livingSegment.Item2)
            {
                // lineSegment in livingSegment
                if (pointIsInSegment(point, Points[index - 1], Points[index]))
                {
                    return true;
                }
            }
            else if (livingSegment.Item1 >= lineSegment.Item1 && livingSegment.Item2 <= lineSegment.Item2)
            {
                // livingSegment in lineSegment
                if (pointIsInSegment(point,
                    Points[index - 1] + lineDirection * (livingSegment.Item1 - lineSegment.Item1),
                    Points[index - 1] + lineDirection * (livingSegment.Item2 - lineSegment.Item1)))
                {
                    return true;
                }
            }
            else if (livingSegment.Item2 < lineSegment.Item2)
            {
                // livingSegment to the left of lineSegment
                if (pointIsInSegment(point,
                    Points[index - 1],
                    Points[index - 1] + lineDirection * (livingSegment.Item2 - lineSegment.Item1)))
                {
                    return true;
                }
            }
            else if (livingSegment.Item1 > lineSegment.Item1)
            {
                // livingSegment to the right of lineSegment
                if (pointIsInSegment(point,
                    Points[index - 1] + lineDirection * (livingSegment.Item1 - lineSegment.Item1),
                    Points[index]))
                {
                    return true;
                }
            }
            else if (OS.IsDebugBuild())
            {
                throw new System.Exception(System.String.Format("({0} -:- {1}) \\ ({2} -:- {1})",
                    lineSegment.Item1, lineSegment.Item2, livingSegment.Item1, livingSegment.Item2));
            }
        }
        return false;
    }

    private bool pointIsInSegment(Vector2 point, Vector2 segmentFirst, Vector2 segmentSecond)
    {
        var closestPoint = Geometry.GetClosestPointToSegment2d(point, segmentFirst, segmentSecond);
        return closestPoint.DistanceSquaredTo(point) <= Width * Width / 4;
    }

    private Rect2 getBoundingRectangle()
    {
        if (frozen)
        {
            return precalculatedBoundingRect;
        }
        Vector2 minPoint = Points[0];
        Vector2 maxPoint = Points[0];
        for (int index = 1; index < Points.Length; index++)
        {
            minPoint = new Vector2(Mathf.Min(minPoint.x, Points[index].x), Mathf.Min(minPoint.y, Points[index].y));
            maxPoint = new Vector2(Mathf.Max(maxPoint.x, Points[index].x), Mathf.Max(maxPoint.y, Points[index].y));
        }
        return new Rect2(minPoint, maxPoint - minPoint);
    }
}
