using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink.Health;
using MrPink.WeaponsSystem;
using UnityEngine;

public class NetworkProjectileSpawner : NetworkBehaviour
{
    public static NetworkProjectileSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnProjectileOnEveryClient(float noiseDistance, Pooling.AttackColliderPool.AttackColliderPrefabTag _attackColliderTag, Transform shotHolder, Vector3 targetPos, Vector3 direction, HealthController ownerHc, DamageSource source, float offsetX, float offsetY)
    {
        if (_attackColliderTag == Pooling.AttackColliderPool.AttackColliderPrefabTag.PlayerSword ||
            _attackColliderTag == Pooling.AttackColliderPool.AttackColliderPrefabTag.DesertBeast ||
            _attackColliderTag == Pooling.AttackColliderPool.AttackColliderPrefabTag.DesertBeastSmall)
        {
            SpawnMeleeProjectileLocally( shotHolder, _attackColliderTag, direction,  ownerHc, source, offsetX, offsetY);   
        }
        else // RANGED
        {
            RpcSpawnProjectileOnEveryClient_Server( noiseDistance, shotHolder.position, _attackColliderTag, direction,  ownerHc, source, offsetX, offsetY);   
        }
    }

    void SpawnMeleeProjectileLocally(Transform shotHolder, Pooling.AttackColliderPool.AttackColliderPrefabTag _attackColliderTag, Vector3 direction, HealthController ownerHc, DamageSource source, float offsetX, float offsetY)
    {
        BaseAttackCollider newProjectile;
        newProjectile = Pooling.Instance.SpawnProjectile(_attackColliderTag, shotHolder, Vector3.zero, Quaternion.identity);
        newProjectile.Init(ownerHc, source, shotHolder, ScoringActionType.NULL, offsetX, offsetY);
    }

    [ServerRpc(RequireOwnership = false)]
    void RpcSpawnProjectileOnEveryClient_Server(float noiseDistance, Vector3 pos,Pooling.AttackColliderPool.AttackColliderPrefabTag _attackColliderTag, Vector3 direction, HealthController ownerHc, DamageSource source, float offsetX, float offsetY)
    {
        NoiseSystem.Instance.MakeNoise(pos, noiseDistance);
        RpcSpawnProjectileOnEveryClient_Client(pos, _attackColliderTag, direction,  ownerHc, source, offsetX, offsetY);
    }
    [ObserversRpc]
    void RpcSpawnProjectileOnEveryClient_Client(Vector3 pos, Pooling.AttackColliderPool.AttackColliderPrefabTag _attackColliderTag, Vector3 direction, HealthController ownerHc, DamageSource source, float offsetX, float offsetY)
    {
        BaseAttackCollider newProjectile;
            newProjectile = Pooling.Instance.SpawnProjectile(_attackColliderTag, null, pos,
                Quaternion.LookRotation(direction));

        //newProjectile.Init(ownerHc, source, shotHolder, action);
        newProjectile.Init(ownerHc, source, null, ScoringActionType.NULL, offsetX, offsetY);
    }
}
