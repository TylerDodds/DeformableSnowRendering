// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{
    public static class HeightmapMeshGenerator
    {
        public static Mesh GenerateMesh(Texture2D heightMap, Vector2 horizontalSize, float heightMultiplier, Vector2Int numDivisionsHorizontal)
        {
            int meshWidth = numDivisionsHorizontal.x + 1;
            int meshHeight = numDivisionsHorizontal.y + 1;
            Vector3[] vertices = new Vector3[meshWidth * meshHeight];
            Vector2[] uvs = new Vector2[meshWidth * meshHeight];
            int[] triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];

            int triangleIndex = 0;
            int vertexIndex = 0;
            void AddTriangle(int a, int b, int c)
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }

            Vector2 uvDenominators = new Vector2(1f / (meshWidth - 1), 1f / (meshHeight - 1));
            for (int y = 0; y < meshHeight; y += 1)
            {
                for (int x = 0; x < meshWidth; x += 1)
                {
                    Vector2 uv = new Vector2(x * uvDenominators.x, y * uvDenominators.y);
                    vertices[vertexIndex] = new Vector3(horizontalSize.x * (uv.x - 0.5f), heightMap.GetPixelBilinear(uv.x, uv.y).r * heightMultiplier, horizontalSize.y * (uv.y - 0.5f));
                    uvs[vertexIndex] = uv;

                    if (x < meshWidth - 1 && y < meshHeight - 1)
                    {
                        AddTriangle(vertexIndex, vertexIndex + meshWidth, vertexIndex + meshWidth + 1);
                        AddTriangle(vertexIndex + meshWidth + 1, vertexIndex + 1, vertexIndex);
                    }

                    vertexIndex++;
                }
            }


            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }


}