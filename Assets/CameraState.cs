using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class CameraState : MonoBehaviour
{
    public enum StateName {None, Follow, Sceneic, Castle, MouseFollow, Grave};
    public enum ParentLookTarget { None, PlayerForward, Obj, Mouse};

    public static CameraState i;
    void Awake() { i = this; }

    [System.Serializable]
    public class State
    {
        [HideInInspector] public string name;
        public StateName displayName;
        [HideInInspector] public bool selected;

        [Range(0, 1)] public float playerX, playerY, zoom = 0.5f;
        public Vector2 limitsX = new Vector2(-1, 1), limitsY = new Vector2(-1, 1), zoomLimits = new Vector2(0, 10);
        public Vector3 camTargetOffset = Vector3.zero, camParentPlayerOffset = Vector3.zero;
        public bool lookAtPlayer = true, followPlayer = true;
        public ParentLookTarget parentLookTarget;

        [Header("Face Player Forwards")]
        public float parentRotSmoothness = 0.25f;

        [Header("Object Focus")]
        public GameObject objFocus;

        [Header("Mouse Focus")]
        public float mouseXSens = 1;
        public float mouseYSens = 1;

        public State() { }

        public State(State original)
        {
            name = original.name;
            displayName = original.displayName;
            selected = original.selected;
            playerX = original.playerX;
            playerY = original.playerY;
            zoom = original.zoom;
            limitsX = original.limitsX;
            limitsY = original.limitsY;
            zoomLimits = original.zoomLimits;
            camTargetOffset = original.camTargetOffset;
            camParentPlayerOffset = original.camParentPlayerOffset;
            lookAtPlayer = original.lookAtPlayer;
            followPlayer = original.followPlayer;
            parentLookTarget = original.parentLookTarget;
            parentRotSmoothness = original.parentRotSmoothness;
            objFocus = original.objFocus;
        }

        public bool equals(State o)
        { 
            if (lookAtPlayer != o.lookAtPlayer) return false;
            if (followPlayer != o.followPlayer) return false;
            
            if (playerX != o.playerX) return false;
            if (playerY != o.playerY) return false;
            if (zoom != o.zoom) return false;
            if (parentRotSmoothness != o.parentRotSmoothness) return false;

            if (parentLookTarget != o.parentLookTarget) return false;
            if (objFocus != o.objFocus) return false;

            if (Vector2.Distance(limitsX, o.limitsX) > 0.01f) return false;
            if (Vector2.Distance(limitsY, o.limitsY) > 0.01f) return false;
            if (Vector2.Distance(zoomLimits, o.zoomLimits) > 0.01f) return false;

            if (Vector3.Distance(camTargetOffset, o.camTargetOffset) > 0.1f) return false;
            if (Vector3.Distance(camParentPlayerOffset, o.camParentPlayerOffset) > 0.1f) return false;

            return true;
        }
    }

    [SerializeField] int selectedState;
    [SerializeField] bool nextState, prevState; 

    [SerializeField] List<State> states = new List<State>();
    public State current { get { return GetCurrent(); } }
    public bool mouseControl { get { return current.parentLookTarget == ParentLookTarget.Mouse && GetComponent<CameraController>().mouseTransitionTimeLeft < 0;  } }

    public void AddState(State state, int stateNum)
    {
        if (stateNum == -1 || stateNum >= states.Count) states.Add(state);
        else {
            var name = states[stateNum].displayName;
            states[stateNum] = state;
            states[stateNum].displayName = name;
        }

        FindSelectedState();
    }

    public void SwitchToState(StateName stateName)
    {
        for (int i = 0; i < states.Count; i++) if (states[i].displayName == stateName) { SwitchToState(i); return; }
    }

    public void SwitchToState(int stateID)
    {
        if (stateID == -1) return;

        selectedState = stateID;
        FindSelectedState();
    }

    private void OnValidate()
    {
        FindSelectedState();
    }

    private void Update()
    {
        if (nextState) {
            nextState = false;
            if (selectedState < states.Count-1) selectedState += 1;
            FindSelectedState();
        }
        if (prevState) {
            prevState = false;
            if (selectedState > 0) selectedState -= 1;
            FindSelectedState();
        }
    }

    void FindSelectedState() 
    {
        if (selectedState < states.Count) {
            foreach (var s in states) s.selected = false;
            states[selectedState].selected = true;
        }
        bool found = false;
        for (int i = 0; i < states.Count; i++) {
            if (states[i].selected) {
                if (found) states[i].selected = false;
                else {
                    found = true;
                    selectedState = i;
                }
            }
            if (states[i].displayName == StateName.None) states[i].name = ("state " + i + ": ") + (states[i].selected ? "current" : "");
            else states[i].name = "state " + i + " - " + states[i].displayName + ": " + (states[i].selected ? "current" : "");
        }
    }

    private void Start()
    {
        FindSelectedState();
    }

    State GetCurrent()
    {
        foreach (var s in states) if (s.selected) return s;
        return null;
    }
}
