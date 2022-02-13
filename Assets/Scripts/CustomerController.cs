using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// simple controller hold couple of customers
/// </summary>
public class CustomerController : MonoBehaviour
{
    [System.Serializable]
    public class CustomerInfo
    {
        /// <summary>
        /// for sprite animations
        /// </summary>
        public Sprite AcceptSprite;
        public Sprite RejectSprite;
        public Sprite DefaultSprite;
    }

    public List<CustomerInfo> CustomerInfoTable = new List<CustomerInfo>();

    /// <summary>
    /// randomly pick a visually different customer
    /// </summary>
    public CustomerInfo GetRandomCustomerInfo()
    {
        return CustomerInfoTable[UnityEngine.Random.Range(0, CustomerInfoTable.Count)];
    }
}
