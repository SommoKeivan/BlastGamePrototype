using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Graphics
{
    /// <summary>
    /// Class representing the graphical version of a GameBlock.
    /// It requires the components: Button and Animator.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Animator))]
    public class GameBlockUI : MonoBehaviour
    {
        [SerializeField] public AudioClip interactionAudioClip;
    
        private Button _button;
        protected Animator Animator;

        private static readonly int DestroyAnimation = Animator.StringToHash("DestroyAnimation");

        public void SetImageColor(Color newColor) => _button.image.color = newColor;
        public void SetOnClickListener(UnityAction action) => _button.onClick.AddListener(action);

    
    
        private void Awake()
        {
            _button = GetComponent<Button>();
            Animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Method that plays a "death" animation, waits for its end, and destroys the object.
        /// </summary>
        /// <returns>An IEnumerator to manage the death</returns>
        public IEnumerator BlockDestruction()
        {
            // Play the animation
            Animator.SetTrigger(DestroyAnimation);
        
            // Wait for the animation to end
            yield return new WaitWhile(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        
            // Destroy the GameBlockUI after the animation is complete
            Destroy(gameObject);
        }
    }
}