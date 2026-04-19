using System.Collections.Generic;
using D_Dev.EntitySpawner;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    [CreateAssetMenu(menuName = "Game/Queue/Visitor Container", fileName = "VisitorContainer")]
    public class VisitorContainer : ScriptableObject
    {
        #region Fields

        [SerializeField] private List<EntitySpawnSettings> _visitors = new();

        #endregion

        #region Properties

        public IReadOnlyList<EntitySpawnSettings> Visitors => _visitors;
        public int Count => _visitors.Count;

        #endregion

        #region Public

        public EntitySpawnSettings GetRandom()
        {
            if (_visitors.Count == 0)
                return null;

            int startIndex = Random.Range(0, _visitors.Count);
            for (int i = 0; i < _visitors.Count; i++)
            {
                var settings = _visitors[(startIndex + i) % _visitors.Count];
                if (settings != null)
                    return settings;
            }

            return null;
        }

        #endregion
    }
}
