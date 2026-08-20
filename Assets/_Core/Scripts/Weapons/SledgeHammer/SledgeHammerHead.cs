using UnityEngine;

namespace HexaBit.Core {
    public class HammerHeadCollision : MonoBehaviour {
        [SerializeField] private SledgeHammerEffect parentEffect;

        private void Start() {
            // Busca o pai automaticamente se não estiver atribuído
            if (parentEffect == null)
                parentEffect = GetComponentInParent<SledgeHammerEffect>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (parentEffect != null) {
                parentEffect.OnHammerHit(other);
            }
        }
    }
}