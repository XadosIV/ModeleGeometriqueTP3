using UnityEngine;

public class VoxelSphereGizmos : MonoBehaviour
{
    public enum operation { Union, Intersection }
    public operation op = operation.Union;

    public int resolution = 20;
    public float radius = 1f;

    [System.Serializable]
    public struct Sphere
    {
        public Vector3 center;
        public float radius;
    }

    public Sphere[] spheres;

    public GameObject prefab;

    public bool draw = true;

    void OnDrawGizmos()
    {
        if (!draw) return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }


        // Boite englobante
        Vector3 min = spheres[0].center - Vector3.one * spheres[0].radius;
        Vector3 max = spheres[0].center + Vector3.one * spheres[0].radius;
        foreach (var s in spheres)
        {
            min = Vector3.Min(min, s.center - Vector3.one * s.radius);
            max = Vector3.Max(max, s.center + Vector3.one * s.radius);
        }

        Vector3 size = max - min;
        float voxelSize = size.x / resolution;

        Vector3 origin = transform.position - Vector3.one * radius;

        for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 center = origin + new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * voxelSize;

                    bool final = false;

                    // Vérifier si le centre est dans une des sphères
                    bool insideUnion = false;
                    bool insideIntersection = true;
                    foreach (var s in spheres)
                    {
                        if ((center - s.center).sqrMagnitude <= s.radius * s.radius)
                        {
                            insideUnion = true;
                        }
                        else
                        {
                            insideIntersection = false;
                        }

                        final = op == operation.Union ? insideUnion : insideIntersection;
                    }

                    if (final)
                    {
                        GameObject go = Instantiate(prefab, center, Quaternion.identity, transform);
                        go.transform.localScale = Vector3.one * voxelSize;
                        
                    }
                }
        draw = false;
    }
}
