using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TerrainZoomout : MonoBehaviour
{
    TerrainData _terrainData;
    float[,] _heightMap;
    
    int _width;
    [Range(1f, 10f)]
    public float _scale = 2f;
    [Range(0.1f, 10f)]
    public float _centerScale = 3f;
    [Range(1, 8)]
    public int iteration = 2;
    [Range(0f, 1f)]
    public float arcFactor = 0;
    public PlayableDirector Director;
    float timer = 0f;

    private void Start() {
        _terrainData = Instantiate(Terrain.activeTerrain.terrainData);
        Terrain.activeTerrain.terrainData = _terrainData;
        _width = _terrainData.heightmapResolution;
        _heightMap = new float[_width, _width];
        Director.Play();
    }

    public void ScaleDown(){
        float rad = Mathf.Deg2Rad * (arcFactor * 80f + 10f);
        float r = 0.5f / Mathf.Sin(rad);
        float d = Mathf.Cos(rad) * r;
        for (int i = 0; i < _width; i++){
            for (int j = 0; j < _width; j++){
                float xcenter = (float)j / _width - 0.5f;
                float ycenter = (float)i / _width - 0.5f;
                for (int k = 0; k < iteration; k++) {
                    float iFactor = Mathf.Pow(2, k);
                    float x = xcenter * _centerScale * iFactor + 0.5f;
                    float y = ycenter * _centerScale * iFactor + 0.5f;
                    float height = Mathf.PerlinNoise(x * _scale, y * _scale);
                    if(k == 0){
                        _heightMap[j, i] = height * 0.3f;
                    }
                    else{
                        _heightMap[j, i] += height * 0.3f * Mathf.Pow(0.5f, k+1);
                    }
                }
                float h = Mathf.Sqrt(Mathf.Pow(r, 2f) - Mathf.Pow(xcenter, 2f)) - d;
                _heightMap[j, i] = arcFactor * h + (1f - arcFactor) * _heightMap[j, i];
            }
        }
        _terrainData.SetHeights(0, 0, _heightMap);
    }

    private void FixedUpdate() {

    }

    private void OnValidate() {
    }


}
