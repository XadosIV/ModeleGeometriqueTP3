using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Cube
{
    public Vector3 center;
    public Vector3 scale;
}

public class Octree
{
    private Octree[] nodes;
    private bool isLeaf;
    private bool filled;
    private Vector3 center;
    private float size;
    private bool intersectionOperation; // true => intersection, false => union

    public Octree(Vector3 center, float size, int maxDepth, bool intersectionOperation = false)
    {
        this.center = center;
        this.size = size;
        this.filled = false;
        this.intersectionOperation = intersectionOperation;

        if (maxDepth <= 0)
        {
            isLeaf = true;
            nodes = null;
        }
        else
        {
            isLeaf = false;
            nodes = new Octree[8];

            float half = size / 2f;
            float quarter = size / 4f;

            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(
                    ((i & 1) == 0 ? -1 : 1) * quarter,
                    ((i & 2) == 0 ? -1 : 1) * quarter,
                    ((i & 4) == 0 ? -1 : 1) * quarter
                );

                nodes[i] = new Octree(center + offset, half, maxDepth - 1, intersectionOperation);
            }
        }
    }

    // Détermine la relation entre la sphère et ce cube :
    // 0 = complètement dehors, 1 = complètement dedans, 2 = partiel (intersecte)
    int SphereCubeRelation(Vector3 sphereCenter, float radius)
    {
        float half = size / 2f;

        // bornes du cube
        Vector3 min = center - Vector3.one * half;
        Vector3 max = center + Vector3.one * half;

        // 1) Test rapide : distance du point au cube (distance minimale entre le centre de la sphère
        // et n'importe quel point du cube). Si cette distance > radius => pas d'intersection.
        float distSq = 0f;
        for (int i = 0; i < 3; i++)
        {
            float v = sphereCenter[i];
            if (v < min[i])
            {
                float d = min[i] - v;
                distSq += d * d;
            }
            else if (v > max[i])
            {
                float d = v - max[i];
                distSq += d * d;
            }
        }
        if (distSq > radius * radius) return 0; // complètement dehors

        // 2) Tester si tous les sommets du cube sont à l'intérieur de la sphère => cube entièrement dedans
        Vector3[] corners = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z)
        };

        float rSq = radius * radius;
        foreach (var c in corners)
        {
            if ((c - sphereCenter).sqrMagnitude > rSq)
            {
                // au moins un coin est hors de la sphère -> intersection partielle
                return 2;
            }
        }

        // tous les coins sont à l'intérieur -> cube entièrement contenu
        return 1;
    }


    public void Build(List<Sphere> spheres)
    {
        if (!intersectionOperation)
        {
            // Union
            bool fullyInsideAny = false;
            bool fullyOutsideAll = true;

            foreach (var sphere in spheres)
            {
                int rel = SphereCubeRelation(sphere.center, sphere.radius);
                if (rel == 1) fullyInsideAny = true;
                if (rel != 0) fullyOutsideAll = false;
            }

            if (fullyOutsideAll)
            {
                isLeaf = true;
                filled = false;
                nodes = null;
                return;
            }

            if (fullyInsideAny)
            {
                isLeaf = true;
                filled = true;
                nodes = null;
                return;
            }

            // partiellement dedans
            if (nodes != null)
            {
                foreach (var child in nodes)
                    child.Build(spheres);

                bool allLeaves = true;
                bool allFilled = true;
                foreach (var child in nodes)
                {
                    if (!child.isLeaf) allLeaves = false;
                    if (!child.filled) allFilled = false;
                }

                if (allLeaves && allFilled)
                {
                    isLeaf = true;
                    filled = true;
                    nodes = null;
                }
            }
        }
        else
        {
            // Intersection
            bool fullyOutsideAny = false;
            bool fullyInsideAll = true;

            foreach (var sphere in spheres)
            {
                int rel = SphereCubeRelation(sphere.center, sphere.radius);
                if (rel == 0) fullyOutsideAny = true;
                if (rel != 1) fullyInsideAll = false;
            }

            if (fullyOutsideAny)
            {
                isLeaf = true;
                filled = false;
                nodes = null;
                return;
            }

            if (fullyInsideAll)
            {
                isLeaf = true;
                filled = true;
                nodes = null;
                return;
            }

            // partiellement dedans
            if (nodes != null)
            {
                foreach (var child in nodes)
                    child.Build(spheres);

                bool allLeaves = true;
                bool allFilled = true;
                foreach (var child in nodes)
                {
                    if (!child.isLeaf) allLeaves = false;
                    if (!child.filled) allFilled = false;
                }

                if (allLeaves && allFilled)
                {
                    isLeaf = true;
                    filled = true;
                    nodes = null;
                }
            }
        }
    }

    public List<Cube> Draw()
    {
        List<Cube> cubes = new List<Cube>();
        if (isLeaf)
        {
            if (filled)
            {
                cubes.Add(new Cube { center = center, scale = Vector3.one * size });
            }
        }
        else if (nodes != null)
        {
            foreach (var child in nodes)
            {
                List<Cube> arrayOfCubes = child.Draw();
                cubes.AddRange(arrayOfCubes);
            }
        }
        return cubes;
    }

}
