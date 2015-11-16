using UnityEngine;
using System.Collections;

//ポップアップのようににゅっと出てくるUIに変更できる
//次にポップアップするやつも指定できる
public class SmoothUIAppearer : MonoBehaviour {


    public float InitSize = 0.2f;
    public float ReSizingTime = 0.4f;
    public SmoothUIAppearer NextUI = null;
    public bool WhenFinishedDestroyThisScript = true;
    public SPEEDTYPE SpeedType = SPEEDTYPE.UseEditorValue; 

    public enum SPEEDTYPE {
        UseEditorValue,Lightning,SuperQuickly,Quickly,Speedy,Normal,Slowly 
    }
    void ChangeSpeedBySPEEDTYPE() {
        switch(SpeedType){
            case SPEEDTYPE.UseEditorValue: break;
            case SPEEDTYPE.Lightning:
                ReSizingTime = 0.08f; break;
            case SPEEDTYPE.SuperQuickly:
                ReSizingTime = 0.13f;break;
            case SPEEDTYPE.Quickly:
                ReSizingTime = 0.2f;break;
            case SPEEDTYPE.Speedy:
                ReSizingTime = 0.3f;break;
            case SPEEDTYPE.Normal:
                ReSizingTime = 0.4f; break;
            case SPEEDTYPE.Slowly:
                ReSizingTime = 0.72f; break;
        }
    }

    private bool isDisabledNotByMySelf = false;
    public bool IsDisabledNotByMySelf {
        get { return isDisabledNotByMySelf; }
        set { isDisabledNotByMySelf = value; }
    }

    private float StartTime = 0f;
    public enum CurveType {Linear,Square ,Pop}
    public CurveType curvetype = CurveType.Linear;

    //SetActive(true);の時に呼ばれる！
    void OnEnable() {
        if (IsDisabledNotByMySelf) {
            Start();
        }
    }
    void OnDisable() {
        if(!WhenFinishedDestroyThisScript) this.transform.localScale = new Vector3(1, 1, 1) * InitSize;
        IsDisabledNotByMySelf = true;
    }


    void Awake(){
        ChangeSpeedBySPEEDTYPE();
        this.transform.localScale = new Vector3(1,1,1) * InitSize;
        StartTime = Time.time;
    }
    public void Start() {
        if (NextUI != null && NextUI.gameObject.activeSelf) {
            NextUI.Start();
            
            NextUI.gameObject.SetActive(false);
        }
        StartTime = Time.time;
    }

	
	// Update is called once per frame
	void Update () {
        if (Time.time < StartTime + ReSizingTime) {
            
            float Size = 1f;
            float DiffTime = Time.time -StartTime;//0 < DiffTime < ResizeingTime
            
            if (DiffTime > 0) {
                float DiffSize = 1 - InitSize ;
                float Diff = DiffTime / ReSizingTime;//Diff in [0 → 1] as Linear
                float RDiff = 1 - Diff;//RDiff in [1 → 0] as Linear
                float F = Diff;
                switch(curvetype){
                    case CurveType.Linear:
                        F = Diff; break;
                    case CurveType.Square:
                        F = - Diff * (Diff - 2); break;
                    case CurveType.Pop:
                        F = (-25f / 16f) * (Diff * Diff) + 2.5f * Diff;break; 
                }
                Size = InitSize + DiffSize * F;
            }
            
            this.transform.localScale = new Vector3(1, 1, 1) * Size;

        } else {
            this.transform.localScale = new Vector3(1, 1, 1);
            if (NextUI != null) NextUI.gameObject.SetActive(true);
            if(WhenFinishedDestroyThisScript) Destroy(this);
        }
	}
}
