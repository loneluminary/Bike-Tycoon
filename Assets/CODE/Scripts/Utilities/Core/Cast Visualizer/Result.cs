using UnityEngine;

public struct CastResult
{
    public RaycastHit[] Hits { get; private set; }
    public RaycastHit FirstHit => Hits.Length > 0 ? Hits[0] : new RaycastHit();
    public RaycastHit LastHit => Hits.Length > 0 ? Hits[^1] : new RaycastHit();

    public CastResult(params RaycastHit[] hits)
    {
        Hits = hits ?? new RaycastHit[] { };
    }

    public static bool operator true(CastResult _castResult) => _castResult.Hits.Length > 0;

    public static bool operator false(CastResult _castResult) => _castResult.Hits.Length == 0;

    public static bool operator !(CastResult _castResult) => _castResult.Hits.Length == 0;
}

public struct OverlapResult
{
    public Collider[] Hits { get; private set; }
    public Collider FirstHit => Hits.Length > 0 ? Hits[0] : null;
    public Collider LastHit => Hits.Length > 0 ? Hits[^1] : null;

    public OverlapResult(params Collider[] hits)
    {
        Hits = hits;
    }

    public static bool operator true(OverlapResult _castResult) => _castResult.Hits.Length > 0;

    public static bool operator false(OverlapResult _castResult) => _castResult.Hits.Length == 0;

    public static bool operator !(OverlapResult _castResult) => _castResult.Hits.Length == 0;
}