using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ChargeMeter : MonoBehaviour
{
    [Header("spear charge")]
    public Slider ThrowCharge;
    [SerializeField] private GameObject _centralMarkerShine;
    [SerializeField] private Image _centralMarker;
    [SerializeField] Color _centralMarkerDefaultColor;
    [SerializeField] Color _centralMarkerFullColor;
    [SerializeField] private Sound _fullChargeSound;

    private void Start()
    {
        _fullChargeSound = Instantiate(_fullChargeSound);
    }

    public void DisplaySpearCharge(float charge)
    {
        gameObject.SetActive(Player.i.GetComponent<PFighting>().chargingSpear());
        //swCountdown.transform.parent.gameObject.SetActive(Player.i.GetComponent<PFighting>().enabled);

        _centralMarker.color = Color.Lerp(_centralMarkerDefaultColor, _centralMarkerFullColor, charge);

        bool perfect = charge > 0.85 && charge < 1;
        if (perfect && !_centralMarkerShine.activeInHierarchy) _fullChargeSound.Play();
        _centralMarkerShine.SetActive(perfect);
    }
}
