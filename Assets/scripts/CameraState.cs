using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class CameraState : MonoBehaviour
{
    public enum StateName {None, Follow, Sceneic, MouseFollow, MouseOverShoulder, dialogue};
    public enum ParentLookTarget { None, PlayerForward, Obj, Mouse};

    public static CameraState i;
    void Awake() { i = this; }


    private void OnValidate() => FindSelectedState();

    [System.Serializable]
    public class State
    {
        [HideInInspector] public string name;
        public StateName displayName;
        [HideInInspector] public bool selected;
        
        [Header("Camera position")]
        public float camX;
        public float camY, camZ = 0.5f;
        public Vector3 camTargetOffset = Vector3.zero;

        [Header("Parent")]
        public float parentRotSmoothness = 0.25f;
        public ParentLookTarget parentLookTarget;
        public bool followPlayer = true;
        public Vector3 parentLookOffset, parentPosOffset;

        [Header("Object Focus")]
        public GameObject objFocus;
        public int focusIndex;

        [Header("Mouse Focus")]
        public float mouseXSens = 1;
        public float mouseYSens = 1;
        public Vector2 parentRotLimitsY;

        [Header("Misc")]
        public float transitionSmoothness = 0.025f;

        public State() {}

        public State(State original)
        {
            name = original.name;
            displayName = original.displayName;
            selected = original.selected;

            camX = original.camX;
            camY = original.camY;
            camZ = original.camZ;
            camTargetOffset = original.camTargetOffset;

            parentRotSmoothness = original.parentRotSmoothness;
            parentLookTarget = original.parentLookTarget;
            followPlayer = original.followPlayer;
            parentLookOffset = original.parentLookOffset;
            parentPosOffset = original.parentPosOffset;

            objFocus = original.objFocus;
            focusIndex = original.focusIndex;

            mouseXSens = original.mouseXSens;
            mouseYSens = original.mouseYSens;
            parentRotLimitsY = original.parentRotLimitsY;

            transitionSmoothness = original.transitionSmoothness;
        }

        public bool equals(State o)
        {
            if (followPlayer != o.followPlayer) 
                return false;

            if (camX != o.camX || camY != o.camY || camZ != o.camZ || parentRotSmoothness != o.parentRotSmoothness || focusIndex != o.focusIndex ||
                mouseXSens != o.mouseXSens || mouseYSens != o.mouseYSens || transitionSmoothness != o.transitionSmoothness)
                return false;

            if (Vector2.Distance(parentRotLimitsY, o.parentRotLimitsY) > 0.01f)
                return false;

            if (Vector3.Distance(camTargetOffset, o.camTargetOffset) > 0.1f) 
                return false;

            if (Vector3.Distance(parentLookOffset, o.parentLookOffset) > 0.1f) 
                return false; 
            if (Vector3.Distance(parentPosOffset, o.parentPosOffset) > 0.1f) 
                return false;

            return true;
        }

        public void Lerp(State o)
        {
            float smoothness = o.transitionSmoothness;

            focusIndex = o.focusIndex;

            camX = Mathf.Lerp(camX, o.camX, smoothness);
            camY = Mathf.Lerp(camY, o.camY, smoothness);
            camZ = Mathf.Lerp(camZ, o.camZ, smoothness);

            camTargetOffset = Vector3.Lerp(camTargetOffset, o.camTargetOffset, smoothness);
            parentLookOffset = Vector3.Lerp(parentLookOffset, o.parentLookOffset, smoothness);
            parentPosOffset = Vector3.Lerp(parentPosOffset, o.parentPosOffset, smoothness);
            
            parentLookTarget = o.parentLookTarget;
            objFocus = o.objFocus;
            parentRotSmoothness = o.parentRotSmoothness;

            parentRotLimitsY = o.parentRotLimitsY;
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

    public void SetDialogueTarget(Transform speaker, Vector3 Offset)
    {
        foreach (var s in states) {
            if (s.displayName == StateName.dialogue) {
                s.parentLookTarget = ParentLookTarget.Obj;
                s.objFocus = speaker.gameObject;
                s.parentLookOffset = Offset;
            }
        }
    }

    public void SwitchToState(int stateID)
    {
        if (stateID == -1 || stateID == selectedState) return;

        selectedState = stateID;
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
