﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extension {


    public static Vector2 Round(this Vector3 v) {
        int x = Mathf.RoundToInt(v.x);
        int y = Mathf.RoundToInt(v.y);
        return new Vector2(x, y);
    }
}


public class Model : MonoBehaviour {

    private const int kScoreStep = 100;
    private int mCurrentLevel;

    public const int kNormalRows = 20;
    public const int kMaxRows = 22;
    public const int kMaxColumns = 10;

    public int Score { get; private set; } 
    public int HighScore { get; private set; } 

    private Transform[,] mMap = new Transform[kMaxColumns, kMaxRows];
    // Use this for initialization
    void Awake () {
		LoadData();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool IsShapePositionValid(Transform shapeTransform) {
        foreach (Transform childTransform in shapeTransform) {
            if (childTransform.tag == "Block") {
                Vector2 position = childTransform.position.Round();
                //地图边界
                if (IsInsideMap(position) == false) {
                    return false;
                }
                //其它shape
                if (mMap[(int) position.x, (int) position.y] != null) {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsInsideMap(Vector2 position) {
        return position.x >= 0 && position.x < kMaxColumns && position.y >= 0;
    }

    public void PlaceShape(Transform shapeTransform) {
        foreach (Transform childTransform in shapeTransform) {
            if (childTransform.tag == "Block") {
                Vector2 position = childTransform.position.Round();
                mMap[(int) position.x, (int) position.y] = childTransform;
            }
        }
        //check
        CheckMap();
    }

    public void CheckMap() {
        int count = 0;//计算行数和分数
        for (int i = 0; i < kMaxRows; i++) {
            bool isFull = CheckIsRowFull(i);
            if (isFull) {
                count++;
                ClearOneRow(i);
                MoveLines(i + 1);
                i--;//消除一行之后更新数据
            }
        }
        if (count > 0) {
            Score += (count * 100);
            if (Score > HighScore) {
                HighScore = Score;
            }
            EventManager.Instance.Fire(UIEvent.REFRESH_SCORE);
            var tempLevel = Score / kScoreStep;
            if (tempLevel > mCurrentLevel) {
                mCurrentLevel = tempLevel;
                GameManager.Instance.UpgradeLevel();

            }

        }
    }

    private bool CheckIsRowFull(int rowIndex) {
        for (int i = 0; i < kMaxColumns; i++) {
            if (mMap[i, rowIndex] == null) {
                return false;
            }
        }
        return true;
    }

    private void ClearOneRow(int rowIndex) {
        AudioManager.Instance.PlayLineClear();
        for (int i = 0; i < kMaxColumns; i++) {
            Destroy(mMap[i, rowIndex].gameObject);
            mMap[i, rowIndex] = null;//地图要置空呐!!
        }
    }

    private void MoveLines(int lineIndex) {
        for (int i = lineIndex; i < kNormalRows; i++) {
            for (int j = 0; j < kMaxColumns; j++) {
                if (mMap[j, i] == null) {
                    continue;
                }
                mMap[j, i - 1] = mMap[j, i];
                mMap[j, i] = null;
                mMap[j, i -1].position += Vector3.down;

            }
        }
    }

    public bool IsGameOver() {
        // if max raw has block
        for (int i = 0; i < kMaxColumns; i++) {
            if (mMap[i, kMaxRows-2] != null) {
                SaveData();
                return true;
            }
        }
        return false;
    }

    public int[] GetScoreInfo() {
        return new[] {HighScore, Score};
    }

    private void LoadData() {
        HighScore = PlayerPrefs.GetInt("HighestScore", 0);
    }

    private void SaveData() {
        PlayerPrefs.SetInt("HighestScore", HighScore);
    }

    public void ClearData() {
        HighScore = 0;
        Score = 0;
        SaveData();

    }

    public void RefreshGame() {
        //map and data
        mMap = new Transform[kMaxColumns, kMaxRows];
        GameManager.Instance.RestartGame();
        Score = 0;
        mCurrentLevel = 0;
        EventManager.Instance.Fire(UIEvent.REFRESH_SCORE);

    }
}