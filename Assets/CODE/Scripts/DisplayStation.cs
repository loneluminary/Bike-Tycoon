using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class DisplayStation : MonoBehaviour
{
    [Title("Station Settings")]
    public float BaseSaleChance = 0.05f;

    [Title("Points")]
    [SerializeField] private Transform bikeAnchor;

    [Title("Sounds")]
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioSource audioSource;

    [Title("Events")]
    public UnityEvent<BikeInstance> OnBikePlaced;
    public UnityEvent<DisplayStation> OnBikeSold;

    [Title("Runtime")]
    [ReadOnly] public GalleryRoom ParentRoom;
    [ReadOnly] public BikeInstance CurrentBike;

    private void Awake()
    {
        ParentRoom = GetComponentInParent<GalleryRoom>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        if (!bikeAnchor) bikeAnchor = transform;
    }

    public void RemoveBike()
    {
        if (CurrentBike) Destroy(CurrentBike.gameObject);
        CurrentBike = null;
    }

    public void SellCurrentBike()
    {
        if (!CurrentBike) return;

        CurrentBike.Sell();
        OnBikeSold?.Invoke(this);
    }

    public BikeInstance SpawnBike(BikeData bikeData, int level = 1)
    {
        if (!BikesManager.Instance.BikeInstancePrefab || !bikeData) return null;

        RemoveBike();

        // Create a bike instance
        var bike = Instantiate(BikesManager.Instance.BikeInstancePrefab, transform.position, transform.rotation);
        if (bike != null)
        {
            bike.name = $"{bikeData.BikeName}_Lv{level}";
            bike.Initialize(bikeData, level);

            CurrentBike = bike;
            bike.transform.position = bikeAnchor.transform.position;
            bike.transform.SetParent(bikeAnchor.transform);

            audioSource?.PlayOneShot(placeSound);

            TaskManager.Instance?.OnBikePlaced(bikeData);

            OnBikePlaced?.Invoke(bike);
        }

        return bike;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = CurrentBike ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}