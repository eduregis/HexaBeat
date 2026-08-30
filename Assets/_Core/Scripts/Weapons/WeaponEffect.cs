using UnityEngine;

/// <summary>
/// Classe base para todos os efeitos de armas (projéteis, golpes, áreas, etc.)
/// </summary>
/// 
namespace HexaBit.Core {
    public abstract class WeaponEffect : MonoBehaviour {
        /// <summary>
        /// Inicializa o efeito com os dados da arma, nível e direção.
        /// </summary>
        /// <param name="data">Dados da arma (WeaponData)</param>
        /// <param name="levelIndex">Nível atual da arma </param>
        /// <param name="direction">Direção para onde o efeito deve ser aplicado (ex: facing direction do herói)</param>
        /// <param name="heroTransform">Posição do herói para posicionar algumas armas</param>
        public abstract void Initialize(WeaponData data, int levelIndex, Vector2 direction, HeroController hero);
    }
}