using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]

public class UIAnimationPlayer : MonoBehaviour {
    public event UnityAction OnFinishAnimation;

    Animator anim;
    AnimationClip playingClip;
    PlayableGraph graph;
    AnimationPlayableOutput output;
    AnimationClipPlayable clipPlayable;

    void OnDestroy() {
        graph.Destroy();
    }

    public void Init() {
        anim = GetComponent<Animator>();

        graph = PlayableGraph.Create();
        output = AnimationPlayableOutput.Create(graph, anim.name, anim);
    }

    public void Play(AnimationClip clip) {
        playingClip = clip;
        clipPlayable = AnimationClipPlayable.Create(graph, clip);

        output.SetSourcePlayable(clipPlayable);

        graph.Play();
        StartCoroutine(WaitAnimationCo());
    }

    IEnumerator WaitAnimationCo() {
        yield return new WaitUntil(() => clipPlayable.GetTime() >= playingClip.length);
        OnFinishAnimation?.Invoke();
    }
}
