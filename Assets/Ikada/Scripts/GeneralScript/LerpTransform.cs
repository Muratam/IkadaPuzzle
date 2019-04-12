using UnityEngine;
using System;
using System.Collections.Generic;

// セットした値にTransformの値を補完してゆっくり変化させる
public class LerpTransform : MonoBehaviour
{
    public float LerpTime = 3f;
    public bool DestroyWhenFinished = false;
    public bool LerpFinished { get; private set; }
    Vector3 dstLocalPosition;
    Vector3 dstEulerAngles;
    float lerpingTime = 0;
    Queue<Action> actions = new Queue<Action>();

    public Vector3 LocalPosition
    {
        set { StartLerp(value, dstEulerAngles); }
    }
    public Vector3 Position
    {
        set { StartLerp(transform.parent ? transform.parent.InverseTransformPoint(value) : value, dstEulerAngles); }
    }
    public Vector3 EulerAngles
    {
        set { StartLerp(dstLocalPosition, value); }
    }
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }
    public void AddAction(Action action)
    {
        actions.Enqueue(action);
    }
    public void ClearActions()
    {
        actions.Clear();
    }

    void StartLerp(Vector3 dstLocalPosition, Vector3 dstEulerAngles)
    {
        lerpingTime = 0f;
        LerpFinished = false;
        this.dstLocalPosition = dstLocalPosition;
        this.dstEulerAngles = ClumpEulerAngle(dstEulerAngles);
        Debug.Log(this.dstEulerAngles);
    }
    float ClumpEulerAngle(float val)
    {
        return val < -1e-4 ? val + 360 : val;
    }
    Vector3 ClumpEulerAngle(Vector3 val)
    {
        return new Vector3(ClumpEulerAngle(val.x), ClumpEulerAngle(val.y), ClumpEulerAngle(val.z));
    }
    Vector3 Lerp(Vector3 src, Vector3 dst, float Per)
    {
        return src * (1 - Per) + dst * Per;
    }

    void Awake() { Init(); }
    public void Init(bool lerpFinished = false)
    {
        lerpingTime = LerpTime;
        LerpFinished = lerpFinished;
        dstLocalPosition = transform.localPosition;
        dstEulerAngles = Vector3.zero;
    }
    void Update()
    {
        lerpingTime += Time.deltaTime;
        if (lerpingTime < LerpTime)
        {
            float per = lerpingTime / LerpTime;
            transform.localPosition = Lerp(transform.localPosition, dstLocalPosition, per);
            transform.eulerAngles = Lerp(transform.eulerAngles, dstEulerAngles, per);
        }
        else if (!LerpFinished)
        {
            LerpFinished = true;
            transform.localPosition = dstLocalPosition;
            transform.eulerAngles = dstEulerAngles;
            Debug.Log("DEST");
            Debug.Log(this.dstEulerAngles);
            foreach (var action in actions) action();
            actions.Clear();
            if (DestroyWhenFinished) Destroy(this);
        }
    }
}
