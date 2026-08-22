using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class LightningBoltWeapon : WeaponBase {
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            int extraTargets = data.GetInt(currentLevel, DynamicParameter.ExtraTargets);
            float splashArea = data.GetFloat(currentLevel, DynamicParameter.SplashArea);

            List<Transform> targets = GetValidTargets(extraTargets);
            if (targets.Count == 0) return;

            int maxBolts = Mathf.Min(targets.Count, extraTargets + 1);
            for (int i = 0; i < maxBolts; i++) {
                Transform target = targets[i];
                if (target == null) continue;

                GameObject boltGO = Instantiate(data.attackPrefab, target.position, Quaternion.identity);
                LightningBoltEffect effect = boltGO.GetComponent<LightningBoltEffect>();
                if (effect != null) {
                    // Inicializa com os dados (não usa direction e heroTransform)
                    effect.Initialize(data, currentLevel, Vector2.zero, hero.transform);
                    // Define o alvo
                    effect.SetTarget(target);
                }
            }
        }

        private List<Transform> GetValidTargets(int extraTargets) {
            // Busca todos os inimigos com tag "Enemy"
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            List<Transform> validTargets = new List<Transform>();

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return validTargets;

            foreach (var enemyGO in enemies) {
                EnemyController enemyCtrl = enemyGO.GetComponent<EnemyController>();
                if (enemyCtrl == null) continue;

                // Verifica se o inimigo está dentro da tela
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemyGO.transform.position);
                if (viewportPos.x >= 0 && viewportPos.x <= 1 &&
                    viewportPos.y >= 0 && viewportPos.y <= 1 &&
                    viewportPos.z > 0) // z > 0 significa que está na frente da câmera
                {
                    validTargets.Add(enemyGO.transform);
                }
            }

            // Ordena por distância (do mais próximo ao mais distante)
            validTargets.Sort((a, b) =>
                Vector3.Distance(transform.position, a.position)
                .CompareTo(Vector3.Distance(transform.position, b.position))
            );

            return validTargets;
        }
    }
}