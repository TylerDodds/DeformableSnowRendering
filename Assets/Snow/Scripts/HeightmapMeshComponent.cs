// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class HeightmapMeshComponent : MonoBehaviour
    {
        [SerializeField]
        private Texture2D _heightMap;

        [SerializeField]
        Vector2Int _numberDivisions = new Vector2Int(10, 10);
        [SerializeField]
        Vector2 _horizontalSize = new Vector2(10, 10);
        [SerializeField]
        float _height = 1f;

        private void Awake()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = null;
            OnValidate();
            UpdateMeshIfNeeded();
        }

        private void OnValidate()
        {
            const int maxNumberDivisions = 200;//Need to be careful about the maximum number of vertices in a mesh that we can create like this.
            _numberDivisions = new Vector2Int(Mathf.Min(maxNumberDivisions, _numberDivisions.x), Mathf.Min(maxNumberDivisions, _numberDivisions.y));
            //No need to set the mesh in OnValidate, just entering or exiting play mode will be fine. Avoids warning about calling SendMessage in OnValidate when setting the MeshFilter's sharedMesh or mesh.
        }

        private void UpdateMeshIfNeeded()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh == null && _heightMap != null)
            {
                Mesh newMesh = HeightmapMeshGenerator.GenerateMesh(_heightMap, _horizontalSize, _height, _numberDivisions);
                newMesh.hideFlags = HideFlags.HideAndDontSave;
                meshFilter.sharedMesh = newMesh;
            }
        }
    }
}