using System.Collections;
using UnityEngine;
namespace Graphics
{
    /// <summary>
    /// GameBlockUI extension class. It provides a method for playing a second "death" animation. This allows for an
    /// "active" animation, which is when the object causes the interaction, and a "passive" one, which is when the
    /// object is destroyed as a result of an interaction caused by another GameBlockUI.
    /// </summary>
    public class SpecialGameBlockUI : GameBlockUI
    {
        private static readonly int DestroyActiveAnimation = Animator.StringToHash("DestroyActiveAnimation");

        /// <summary>
        /// Method that plays a different "death" animation, waits for its end, and destroys the object.
        /// </summary>
        /// <returns>An IEnumerator to manage the death</returns>
        public IEnumerator ActiveBlockDestruction()
        {
            // Play the animation
            Animator.SetTrigger(DestroyActiveAnimation);
        
            // Wait for the animation to end
            yield return new WaitWhile(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        
            // Destroy the GameBlockUI after the animation is complete
            Destroy(gameObject);
        }
    }
}