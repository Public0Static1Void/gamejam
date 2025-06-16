using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_BloodParticleCollision : MonoBehaviour
{
    public List<Texture2D> stamps;

    private int max_stamps = 4;
    private void OnParticleCollision(GameObject other)
    {
        if (max_stamps > 0 && !other.CompareTag("Enemy"))
        {
            Debug.Log("Collided with: " + other.name);
            StampOnGround(other.transform.position);

            max_stamps--;
        }
    }

    void StampOnGround(Vector3 position)
    {
        Vector3 forward = transform.forward;
        Vector3[] dirs =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down
        };
        GameObject target = null;
        RaycastHit hit = new RaycastHit();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (Physics.Raycast(transform.position, dirs[i], out hit, 1))
            {
                target = hit.transform.gameObject;

                if (target == null) return;

                Texture2D rand_stamp = stamps[Random.Range(0, stamps.Count)];

                rand_stamp = ConvertToEditable(rand_stamp);

                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null) continue;

                Mesh mesh = meshCollider.sharedMesh;
                Material mat = target.GetComponent<MeshRenderer>().material;

                if (!mat.shader.name.Contains("sh_Stamp")) continue;

                Texture2D mainTex = ConvertToEditable((Texture2D)mat.GetTexture("_DecalTexture"));
                mat.SetTexture("_DecalTexture", mainTex);

                // Triangle info
                int triIndex = hit.triangleIndex;
                int[] triangles = mesh.triangles;
                Vector3[] vertices = mesh.vertices;
                Vector2[] uvs = mesh.uv;

                int i0 = triangles[triIndex * 3 + 0];
                int i1 = triangles[triIndex * 3 + 1];
                int i2 = triangles[triIndex * 3 + 2];

                Transform t = target.transform;
                Vector3 p0 = t.TransformPoint(vertices[i0]);
                Vector3 p1 = t.TransformPoint(vertices[i1]);
                Vector3 p2 = t.TransformPoint(vertices[i2]);

                Vector2 uv0 = uvs[i0];
                Vector2 uv1 = uvs[i1];
                Vector2 uv2 = uvs[i2];

                float avgWorldDist = (Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p0)) / 3f;
                float avgUvDist = (Vector2.Distance(uv0, uv1) + Vector2.Distance(uv1, uv2) + Vector2.Distance(uv2, uv0)) / 3f;

                float uvPerWorldUnit = avgUvDist / avgWorldDist;
                /*
                float worldEdge = (Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p0)) / 3f;
                float uvEdge = (Vector2.Distance(uv0, uv1) + Vector2.Distance(uv1, uv2) + Vector2.Distance(uv2, uv0)) / 3f;

                float uvPerWorldUnit = uvEdge / worldEdge;
                */

                float worldStampSize = Random.Range(2, 4); /// tamaño del stamp
                worldStampSize *= 2 - (1 - (hit.distance / 5));
                float uvSize = worldStampSize * uvPerWorldUnit;

                // Convierte la UV size a pixeles
                int stampPixelSize = Mathf.RoundToInt(mainTex.width * uvSize * 4);

                int multiplier = 4;

                GameManager.gm.StampTexture(mainTex, rand_stamp, hit.textureCoord, stampPixelSize / multiplier, stampPixelSize, Random.Range(0, 360));
            }
        }
    }

    Texture2D ConvertToEditable(Texture2D source)
    {
        // Create a new Texture2D in a compatible format
        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

        // Copy pixels from source texture
        RenderTexture tmpRT = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, tmpRT);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmpRT;

        copy.ReadPixels(new Rect(0, 0, tmpRT.width, tmpRT.height), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmpRT);

        return copy;
    }
}
