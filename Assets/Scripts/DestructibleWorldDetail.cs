using UnityEngine;
using EditorAttributes;
using DG.Tweening;
using System.Collections.Generic;
using Unity.Netcode;

public class DestructibleWorldDetail : MonoBehaviour
{
    public int ObjectID { get; private set; }

    [ReadOnly, SerializeField]private float CurrHealth;
    [SerializeField]private List<LootDropStruct> drops;
    public float Health;
    public string BreakParticle;

    private void Awake()
    {
        ObjectID = Mathf.RoundToInt(transform.position.x * 1000) + Mathf.RoundToInt(transform.position.y * 1000);
        CurrHealth = Health;
        WorldDetailManager.RegisterObject(this);
    }

    private void OnDestroy()
    {
        WorldDetailManager.UnregisterObject(this);
    }

    /// <summary>
    /// ONLY CALL THIS FROM WORLDDETAILMANAGER not from anywhere else. thats why it has an underscore!!!
    /// </summary>
    public void _RemoveHealth(float damage)
    {
        CurrHealth -= damage;
        if(CurrHealth <= 0)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                for (int i = 0; i < drops.Count; i++)
                {
                    if (Random.value <= drops[i].Chance)
                    {
                        var curr = Instantiate(ItemDatabase.GetItem(drops[i].ItemID).ItemPrefab, transform.position + Vector3.up * 0.75f, Quaternion.identity);
                        curr.NetworkObject.Spawn();
                        curr.InitSavedData();
                    }
                }
            }
            Destroy(gameObject);
        }
        //else
        //{
        //    transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.2f);
        //}
    }

    public void Attack(float damage)
    {
        WorldDetailManager.DoDamage(ObjectID, damage);
    }
}