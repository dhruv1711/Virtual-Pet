using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AnimationPlayer))]
public class AnimationPlayerEditor : Editor
{
    AnimationPlayer myTarget;
    UnityEditor.Animations.ChildAnimatorState[] states;

    public override void OnInspectorGUI()
    {
        // Assign the script to this inspector so that we can play with its properties
        myTarget = (AnimationPlayer)target;
        // create the menu and add items to it
        GenericMenu menu = new GenericMenu();
        
        GetAnimatorStates();

        GUILayout.Label("Animation State Name From Model:");

        foreach(UnityEditor.Animations.ChildAnimatorState b in states)
        {
          GUILayout.Label(b.state.name);
        }


    }

    void GetAnimatorStates()
    {
        // Get a reference to the Animator Controller:
        UnityEditor.Animations.AnimatorController animatorController = myTarget.GetTargetAnimator().runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

        // States on layer 0:
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        states = stateMachine.states;
        
    }



}
