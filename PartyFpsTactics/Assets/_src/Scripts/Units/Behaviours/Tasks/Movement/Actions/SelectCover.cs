using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class SelectCover : BaseUnitAction
    {
        public SharedCoverSpot outputCoverSpot;

        public bool isFromRandomPool;
        public bool isClosestNeeded;

        public override TaskStatus OnUpdate()
        {
            outputCoverSpot.Value = GetCover();
            return TaskStatus.Success;
        }

        private CoverSpot GetCover()
        {
            var goodCoverSpots = isFromRandomPool 
                ? CoverSystem.Instance.GetAllCovers()
                : CoverSystem.Instance.FindCover(transform, selfUnit.UnitVision.visibleEnemies);
            
            if (isClosestNeeded)
                return GetClosestCover(goodCoverSpots);
            
            return goodCoverSpots[Random.Range(0, goodCoverSpots.Count)];

        }

        private CoverSpot GetClosestCover(List<CoverSpot> goodCoverSpots)
        {
            float distance = 1000;
            CoverSpot closest = null;
            foreach (var cover in goodCoverSpots)
            {
                if (cover == null)
                    continue;
                
                float newDistance = Vector3.Distance(cover.transform.position, selfUnit.transform.position);
                
                if (newDistance >= distance)
                    continue;
                
                distance = newDistance;
                closest = cover;
            }

            return closest;
        }
    }
}