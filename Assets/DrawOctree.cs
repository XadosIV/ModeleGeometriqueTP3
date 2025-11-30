using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Sphere
{
    public Vector3 center;
    public float radius;
}

public class DrawOctree : MonoBehaviour
{
    public enum operation { Union, Intersection }
    public operation op = operation.Union;

    public int maxDepth = 5;

    
    public List<Sphere> spheres;

    public bool draw = false;

    private void OnDrawGizmos()
    {
        if (!draw) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Bounding box
        Vector3 min = spheres[0].center - Vector3.one * spheres[0].radius;
        Vector3 max = spheres[0].center + Vector3.one * spheres[0].radius;
        foreach (var s in spheres)
        {
            min = Vector3.Min(min, s.center - Vector3.one * s.radius);
            max = Vector3.Max(max, s.center + Vector3.one * s.radius);
        }
        Vector3 size = max - min;

        Octree tree = new Octree((min + max) / 2f, Mathf.Max(size.x, Mathf.Max(size.y, size.z)), maxDepth, op == operation.Intersection);

        tree.Build(spheres);
        List<Cube> cubes = tree.Draw();

        Debug.Log($"Total cubes: {cubes.Count}");

        foreach (Cube cube in cubes)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = cube.center;
            go.transform.localScale = cube.scale;
            go.transform.parent = transform;
        }

        draw = false;
    }
}
