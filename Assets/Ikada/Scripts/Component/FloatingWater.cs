using UnityEngine;
using System.Collections;

// 水面っぽく上下するやつ
public class FloatingWater : MonoBehaviour
{
    Vector3 BasePos;
    void Start()
    {
        BasePos = transform.localPosition;
        floating = getLocalFloating();
    }
    void Update()
    {
        floating = getLocalFloating();
        transform.localPosition = BasePos + Vector3.up * floating;
    }
    float floating = 0;
    private float getLocalFloating()
    {
        return Mathf.Sin(Time.time / 2f) / 4f;
    }
    public float GetLocalFloating()
    {
        return floating;
    }


}
