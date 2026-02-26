using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Extensions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public static class RuntimeUtilities
{
    public static Vector3 ViewportToWorldPlanePoint(Camera theCamera, float zDepth, Vector2 viewportCord)
    {
        Vector2 angles = ViewportPointToAngle(theCamera, viewportCord);
        float xOffset = Mathf.Tan(angles.x) * zDepth;
        float yOffset = Mathf.Tan(angles.y) * zDepth;
        Vector3 cameraPlanePosition = new Vector3(xOffset, yOffset, zDepth);
        cameraPlanePosition = theCamera.transform.TransformPoint(cameraPlanePosition);
        return cameraPlanePosition;
    }

    public static Vector3 ScreenToWorldPlanePoint(Camera camera, float zDepth, Vector3 screenCoord)
    {
        var point = Camera.main.ScreenToViewportPoint(screenCoord);
        return ViewportToWorldPlanePoint(camera, zDepth, point);
    }

    /// Returns X and Y frustum angle for the given camera representing the given viewport space coordinate.
    public static Vector2 ViewportPointToAngle(Camera cam, Vector2 ViewportCord)
    {
        float adjustedAngle = AngleProportion(cam.fieldOfView / 2, cam.aspect) * 2;
        float xProportion = (ViewportCord.x - .5f) / .5f;
        float yProportion = (ViewportCord.y - .5f) / .5f;
        float xAngle = AngleProportion(adjustedAngle / 2, xProportion) * Mathf.Deg2Rad;
        float yAngle = AngleProportion(cam.fieldOfView / 2, yProportion) * Mathf.Deg2Rad;
        return new Vector2(xAngle, yAngle);
    }

    /// Distance between the camera and a plane parallel to the viewport that passes through a given point.
    public static float CameraToPointDepth(Camera cam, Vector3 point)
    {
        Vector3 localPosition = cam.transform.InverseTransformPoint(point);
        return localPosition.z;
    }

    public static float AngleProportion(float angle, float proportion)
    {
        float oppisite = Mathf.Tan(angle * Mathf.Deg2Rad);
        float oppisiteProportion = oppisite * proportion;
        return Mathf.Atan(oppisiteProportion) * Mathf.Rad2Deg;
    }

    public static Vector3 GetPointerWorldPosition2D(Vector3 screenPosition = default, Camera worldCamera = null)
    {
        #if !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 mousePos = Pointer.current.position.ReadValue();
        #else
        Vector2 mousePos = Input.mousePosition;
        #endif

        if (screenPosition == default) screenPosition = mousePos;
        if (!worldCamera) worldCamera = Camera.main;

        if (worldCamera)
        {
            screenPosition.z = Mathf.Abs(worldCamera.transform.position.z); // Set Z depth (distance of 2D plane in the world)
            return worldCamera.ScreenToWorldPoint(screenPosition);
        }
        
        return Vector3.zero;
    }
    
    public static Vector3 GetPointerWorldPoint3D(Camera camera)
    {
        #if !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 mousePos = Pointer.current.position.ReadValue();
        #else
        Vector2 mousePos = Input.mousePosition;
        #endif
        
        Ray ray = camera.ScreenPointToRay(mousePos);
    
        // Return the world position where the ray if hits else return plant world point.
        return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : GetPlaneWorldPoint3D(camera);
    }

    public static Vector3 GetPlaneWorldPoint3D(Camera camera)
    {
        #if !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 mousePos = Mouse.current.position.ReadValue();
        #else
        Vector2 mousePos = Input.mousePosition;
        #endif
        
        Ray ray = camera.ScreenPointToRay(mousePos);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    /// Get a random male name and optionally single letter surname
    public static string GetRandomName(bool withSurname = false)
    {
        List<string> firstNameList = new()
        {
            "Gabe", "Cliff", "Tim", "Ron", "Jon", "John", "Mike", "Seth", "Alex", "Steve", "Chris", "Will", "Bill", "James", "Jim",
            "Ahmed", "Omar", "Peter", "Pierre", "George", "Lewis", "Lewie", "Adam", "William", "Ali", "Eddie", "Ed", "Dick", "Robert", "Bob", "Rob",
            "Neil", "Tyson", "Carl", "Chris", "Christopher", "Jensen", "Gordon", "Morgan", "Richard", "Wen", "Wei", "Luke", "Lucas", "Noah", "Ivan", "Yusuf",
            "Ezio", "Connor", "Milan", "Nathan", "Victor", "Harry", "Ben", "Charles", "Charlie", "Jack", "Leo", "Leonardo", "Dylan", "Steven", "Jeff",
            "Alex", "Mark", "Leon", "Oliver", "Danny", "Liam", "Joe", "Tom", "Thomas", "Bruce", "Clark", "Tyler", "Jared", "Brad", "Jason"
        };

        if (!withSurname)
        {
            return firstNameList[Random.Range(0, firstNameList.Count)];
        }

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return firstNameList[Random.Range(0, firstNameList.Count)] + " " + alphabet[Random.Range(0, alphabet.Length)] + ".";
    }

    public static string GetRandomCityName()
    {
        var cityNameList = new List<string>
        {
            "Alabama", "New York", "Old York", "Bangkok", "Lisbon", "Vee", "Agen", "Agon", "Ardok", "Arbok",
            "Kobra", "House", "Noun", "Hayar", "Salma", "Chancellor", "Dascomb", "Payn", "Inglo", "Lorr", "Ringu",
            "Brot", "Mount Loom", "Kip", "Chicago", "Madrid", "London", "Gam",
            "Greenvile", "Franklin", "Clinton", "Springfield", "Salem", "Fairview", "Fairfax", "Washington", "Madison",
            "Georgetown", "Arlington", "Marion", "Oxford", "Harvard", "Valley", "Ashland", "Burlington", "Manchester", "Clayton",
            "Milton", "Auburn", "Dayton", "Lexington", "Milford", "Riverside", "Cleveland", "Dover", "Hudson", "Kingston", "Mount Vernon",
            "Newport", "Oakland", "Centerville", "Winchester", "Rotary", "Bailey", "Saint Mary", "Three Waters", "Veritas", "Chaos", "Center",
            "Millbury", "Stockland", "Deerstead Hills", "Plaintown", "Fairchester", "Milaire View", "Bradton", "Glenfield", "Kirkmore",
            "Fortdell", "Sharonford", "Inglewood", "Englecamp", "Harrisvania", "Bosstead", "Brookopolis", "Metropolis", "Colewood", "Willowbury",
            "Hearthdale", "Weelworth", "Donnelsfield", "Greenline", "Greenwich", "Clarkswich", "Bridgeworth", "Normont",
            "Lynchbrook", "Ashbridge", "Garfort", "Wolfpain", "Waterstead", "Glenburgh", "Fortcroft", "Kingsbank", "Adamstead", "Mistead",
            "Old Crossing", "Crossing", "New Agon", "New Agen", "Old Agon", "New Valley", "Old Valley", "New Kingsbank", "Old Kingsbank",
            "New Dover", "Old Dover", "New Burlington", "Shawshank", "Old Shawshank", "New Shawshank", "New Bradton", "Old Bradton", "New Metropolis", "Old Clayton", "New Clayton"
        };
        return cityNameList[Random.Range(0, cityNameList.Count)];
    }

    public static Vector3 GetRandomPositionInRectangle(Vector3 lowerLeft, Vector3 upperRight)
    {
        return new(Random.Range(lowerLeft.x, upperRight.x), Random.Range(lowerLeft.y, upperRight.y), Random.Range(lowerLeft.z, upperRight.z));
    }
    
    public static Vector3 GetRandomPositionInRectangle(Vector3 center, Vector2 area)
    {
        Vector3 halfSize = new(area.x / 2f, 0f, area.y / 2f);
        return GetRandomPositionInRectangle(center - halfSize, center + halfSize);
    }
    
    public static Vector3 GetRandomPositionInRectangleExcludingCenter(Vector3 center, Vector3 area, float excludeRadius)
    {
        Vector3 lowerLeft = center - area;
        Vector3 upperRight = center + area;
    
        Vector3 randomPos;
        const int maxAttempts = 10; // Safety break to prevent infinite loops
        int attempts = 0;

        do
        {
            randomPos = new Vector3(Random.Range(lowerLeft.x, upperRight.x), center.y, Random.Range(lowerLeft.z, upperRight.z));
            attempts++;
        } 
        while (Vector3.Distance(center, randomPos) < excludeRadius && attempts < maxAttempts);

        return randomPos;
    }

    public static Vector3 GetRandomPositionInRadius(Vector3 center, float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomPoint.x, center.y, center.z + randomPoint.y);
    }

    public static Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, y, z);
    }

    public static Vector3 GetVectorFromAngle(int angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static Vector3 GetVectorFromAngleInt(int angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

    public static float GetAngleFromVectorFloatXZ(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

    public static int GetAngleFromVector(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        int angle = Mathf.RoundToInt(n);

        return angle;
    }

    public static int GetAngleFromVector180(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int angle = Mathf.RoundToInt(n);

        return angle;
    }

    public static Vector3 ApplyRotationToVector(Vector3 vec, Vector3 vecRotation)
    {
        return ApplyRotationToVector(vec, GetAngleFromVectorFloat(vecRotation));
    }

    public static Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

    public static Vector3 ApplyRotationToVectorXZ(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, angle, 0) * vec;
    }

    public static string GetMonthName(int month, bool shorted)
    {
        string name;
        switch (month)
        {
            default:
            case 0:
                name = "January";
                break;
            case 1:
                name = "February";
                break;
            case 2:
                name = "March";
                break;
            case 3:
                name = "April";
                break;
            case 4:
                name = "May";
                break;
            case 5:
                name = "June";
                break;
            case 6:
                name = "July";
                break;
            case 7:
                name = "August";
                break;
            case 8:
                name = "September";
                break;
            case 9:
                name = "October";
                break;
            case 10:
                name = "November";
                break;
            case 11:
                name = "December";
                break;
        }

        return name[..3];
    }

    public static T GetAssetByInstanceId<T>(int instanceId) where T : Object
    {
        return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(asset => asset.GetInstanceID() == instanceId);
    }

    public static GameObject CreatePlaneMesh(int subdivisions, Vector3 scale, Vector3 position = default, Quaternion rotation = default)
    {
        // Create the GameObject and add the necessary components.
        GameObject gameObject = new GameObject("Plane Mesh (Runtime)", typeof(MeshFilter), typeof(MeshRenderer))
        {
            transform =
            {
                position = position,
                rotation = rotation,
                localScale = scale
            }
        };

        gameObject.GetComponent<MeshFilter>().mesh = GeneratePlaneMesh(subdivisions);

        return gameObject;

        // Generate plane mesh with subdivisions.
        Mesh GeneratePlaneMesh(int subdivisions)
        {
            int vertexCount = (subdivisions + 1) * (subdivisions + 1);
            int quadCount = subdivisions * subdivisions;

            // Initialize arrays
            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uv = new Vector2[vertexCount];
            int[] triangles = new int[quadCount * 6]; // 6 indices per quad (2 triangles)

            float stepSize = 1f / subdivisions;

            // Generate vertices, normals, and UVs
            int vertexIndex = 0;
            for (int y = 0; y <= subdivisions; y++)
            {
                for (int x = 0; x <= subdivisions; x++)
                {
                    vertices[vertexIndex] = new Vector3(x * stepSize - 0.5f, 0, y * stepSize - 0.5f); // Centered on (0, 0)
                    normals[vertexIndex] = Vector3.up;
                    uv[vertexIndex] = new Vector2((float)x / subdivisions, (float)y / subdivisions);
                    vertexIndex++;
                }
            }

            // Generate triangles
            int triangleIndex = 0;
            for (int y = 0; y < subdivisions; y++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    int bottomLeft = y * (subdivisions + 1) + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + subdivisions + 1;
                    int topRight = topLeft + 1;

                    // First triangle (bottom-left)
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;

                    // Second triangle (top-right)
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                }
            }

            // Assign data to the mesh.
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                normals = normals, uv = uv,
                triangles = triangles
            };

            mesh.RecalculateBounds();

            return mesh;
        }
    }

    public static void ToggleMouseCursor(bool lockState)
    {
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }

    /// Applies “gravity” to the given object by moving it downward until it hits the nearest mesh.
    /// The object's position will be updated to the intersection point and its up vector aligned with the surface normal.
    public static void ApplyGravity(Transform obj, Vector3 upAxis)
    {
        // Ensure upAxis has a valid default (Vector3.up) if not provided.
        if (upAxis == default) upAxis = obj.up;

        // Cast a ray downward, starting slightly above the object's current position.
        Ray downwardRay = new Ray(obj.position + Vector3.up, Vector3.down);

        // Find all MeshFilters in the scene.
        MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        float bestDistance = Mathf.Infinity;
        Vector3 bestPoint = obj.position;
        Vector3 bestNormal = Vector3.up;

        foreach (MeshFilter mf in meshFilters)
        {
            // Skip if this mesh filter belongs to the object or one of its children.
            if (mf.transform == obj || mf.transform.IsChildOf(obj)) continue;

            Mesh mesh = mf.sharedMesh;
            if (!mesh) continue;

            // Use renderer bounds (if available) to do a quick check.
            Renderer renderer = mf.GetComponent<Renderer>();

            // If no renderer exists, transform the mesh's local bounds.
            var worldBounds = renderer ? renderer.bounds : TransformBounds(mf.transform, mesh.bounds);

            // If the ray doesn't intersect the bounds, skip this mesh.
            if (!worldBounds.IntersectRay(downwardRay)) continue;

            // Perform a detailed ray-mesh intersection test.
            if (RaycastMesh(downwardRay, mesh, mf.transform, out float distance, out Vector3 point, out Vector3 normal))
            {
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPoint = point;
                    bestNormal = normal;
                }
            }
        }

        // If a hit was found, update the object's position and orientation.
        if (bestDistance < Mathf.Infinity)
        {
            Bounds objBounds = obj.CalculateBounds();
            // Compute how far the bottom of the bounds (objBounds.min) is from the object's current position.
            // Then offset the object so that objBounds.min ends up at bestPoint.
            Vector3 offset = bestPoint - objBounds.min;
            obj.position += offset;

            // Compute the rotation delta from the specified upAxis to the bestNormal.
            Quaternion rotationDelta = Quaternion.FromToRotation(upAxis, bestNormal);
            obj.rotation = rotationDelta * obj.rotation;
        }
    }

    /// Performs a per-triangle intersection test on the mesh.
    public static bool RaycastMesh(Ray ray, Mesh mesh, Transform meshTransform, out float hitDistance, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        hitDistance = Mathf.Infinity;
        hitPoint = Vector3.zero;
        hitNormal = Vector3.up;
        bool hitFound = false;

        var vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Iterate over each triangle (3 indices at a time)
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Convert triangle vertices to world space.
            Vector3 v0 = meshTransform.TransformPoint(vertices[triangles[i]]);
            Vector3 v1 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v2 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

            // Test intersection with this triangle.
            if (IntersectRayTriangle(ray, v0, v1, v2, out float t, out _))
            {
                if (t < hitDistance)
                {
                    hitDistance = t;
                    hitPoint = ray.origin + ray.direction * t;
                    // Compute the triangle normal.
                    hitNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                    hitFound = true;
                }
            }
        }

        return hitFound;
    }

    /// Uses the Möller–Trumbore algorithm to test whether a ray intersects a triangle.
    private static bool IntersectRayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float t, out Vector3 barycentric)
    {
        t = 0f;
        barycentric = Vector3.zero;
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 h = Vector3.Cross(ray.direction, edge2);
        float a = Vector3.Dot(edge1, h);
        if (Mathf.Abs(a) < 0.00001f)
            return false; // Ray is parallel to the triangle.

        float f = 1.0f / a;
        Vector3 s = ray.origin - v0;
        float u = f * Vector3.Dot(s, h);
        if (u is < 0.0f or > 1.0f)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(ray.direction, q);
        if (v < 0.0f || u + v > 1.0f)
            return false;

        t = f * Vector3.Dot(edge2, q);
        if (t > 0.00001f)
        {
            barycentric = new Vector3(1 - u - v, u, v);
            return true;
        }

        return false;
    }

    /// Transforms a local-space Bounds into world space.
    private static Bounds TransformBounds(Transform t, Bounds localBounds)
    {
        Vector3 center = t.TransformPoint(localBounds.center);
        // Transform the extents by the absolute values of the local-to-world matrix.
        Vector3 extents = localBounds.extents;
        Vector3 worldExtents = t.TransformVector(extents);
        worldExtents = new Vector3(Mathf.Abs(worldExtents.x), Mathf.Abs(worldExtents.y), Mathf.Abs(worldExtents.z));
        return new Bounds(center, worldExtents * 2);
    }

    public static List<RaycastResult> RaycastUI<T>(Vector2 position, out T component) where T : Component
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
            position = position
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        component = null;
        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent(out T comp))
            {
                component = comp;
                break;
            }
        }

        return results;
    }

    /// Method was used for creating new WheelColliders in Editor.
    public static WheelCollider CreateWheelCollider(Transform vehicle, Transform wheel)
    {
        // If we don't have any wheel models, throw an error.
        if (!wheel)
        {
            Debug.LogError("You haven't selected your Wheel Model. Please your Wheel Model before creating Wheel Colliders. Script needs to know their sizes and positions, aye?");
            return null;
        }

        // Holding default rotation.
        Quaternion currentRotation = vehicle.rotation;

        // Resetting rotation.
        vehicle.rotation = Quaternion.identity;

        // Creating a new game object called Wheel Colliders for all Wheel Colliders, and parenting it to this game object.
        Transform collidersContainer = vehicle.Find("Wheel Colliders");
        if (!collidersContainer)
        {
            collidersContainer = new GameObject("Wheel Colliders").transform;
            collidersContainer.SetParent(vehicle.transform, false);
            collidersContainer.localRotation = Quaternion.identity;
            collidersContainer.localPosition = Vector3.zero;
            collidersContainer.localScale = Vector3.one;
        }

        WheelCollider wheelCollider = new GameObject(wheel.transform.name, typeof(WheelCollider))
        {
            transform =
            {
                position = wheel.transform.position,
                rotation = vehicle.transform.rotation,
                name = wheel.transform.name
            }
        }.GetComponent<WheelCollider>();

        wheelCollider.transform.SetParent(collidersContainer);
        wheelCollider.transform.localScale = Vector3.one;

        Bounds biggestBound = wheel.CalculateBounds();

        wheelCollider.radius = biggestBound.extents.y / vehicle.transform.localScale.y;
        JointSpring spring = wheelCollider.suspensionSpring;

        spring.spring = 35000f;
        spring.damper = 1500f;
        spring.targetPosition = .5f;

        wheelCollider.suspensionSpring = spring;
        wheelCollider.suspensionDistance = .2f;
        wheelCollider.forceAppPointDistance = .1f;
        wheelCollider.mass = 40f;
        wheelCollider.wheelDampingRate = 1f;

        var sidewaysFriction = wheelCollider.sidewaysFriction;
        var forwardFriction = wheelCollider.forwardFriction;

        forwardFriction.extremumSlip = .3f;
        forwardFriction.extremumValue = 1;
        forwardFriction.asymptoteSlip = 1f;
        forwardFriction.asymptoteValue = 1f;
        forwardFriction.stiffness = 1.5f;

        sidewaysFriction.extremumSlip = .3f;
        sidewaysFriction.extremumValue = 1;
        sidewaysFriction.asymptoteSlip = 1f;
        sidewaysFriction.asymptoteValue = 1f;
        sidewaysFriction.stiffness = 1.5f;

        wheelCollider.sidewaysFriction = sidewaysFriction;
        wheelCollider.forwardFriction = forwardFriction;

        vehicle.transform.rotation = currentRotation;

        return wheelCollider;
    }
    
    private const string keys = "a0b1c2d3e4f56789";
    public static string GetRandomKey()
    {
        string key = "";
        for (int i = 0; i < 8; i++)
        {
            key += keys[Random.Range(0, keys.Length)];
        }
        return key;
    }

    #region Modules

    /// Triggers an Action after a certain time.
    public class FunctionTimer
    {
        private static List<FunctionTimer> timerList;
        private static GameObject initGameObject;

        private readonly GameObject gameObject;
        private float timer;
        private readonly string functionName;
        private readonly bool useUnscaledDeltaTime;
        private readonly Action action;

        #region Initialization

        private static void TryInitialize()
        {
            if (!initGameObject)
            {
                initGameObject = new GameObject("FunctionTimer_Global");
                timerList = new List<FunctionTimer>();
            }
        }

        #endregion

        #region Public Static API

        /// Creates and starts a new FunctionTimer.
        public static FunctionTimer Create(Action action, float time, string functionName = "", bool useUnscaledDeltaTime = false, bool stopAllWithSameName = false)
        {
            TryInitialize();

            if (stopAllWithSameName) StopAllTimersWithName(functionName);

            var obj = new GameObject("FunctionTimer Object " + functionName, typeof(MonoBehaviourHook));
            var funcTimer = new FunctionTimer(obj, action, time, functionName, useUnscaledDeltaTime);

            obj.GetComponent<MonoBehaviourHook>().OnUpdate = funcTimer.Update;
            obj.hideFlags = HideFlags.HideInHierarchy;
            timerList.Add(funcTimer);

            return funcTimer;
        }

        public static void StopAllTimersWithName(string functionName)
        {
            TryInitialize();
            // Iterate backwards for safe removal.
            for (int i = timerList.Count - 1; i >= 0; i--)
            {
                if (timerList[i].functionName == functionName)
                    timerList[i].DestroySelf();
            }
        }

        public static void StopFirstTimerWithName(string functionName)
        {
            TryInitialize();
            var timer = timerList.FirstOrDefault(t => t.functionName == functionName);
            timer?.DestroySelf();
        }

        /// Creates a timer object that must be updated manually.
        public static FunctionTimerObject CreateObject(Action callback, float timer) => new(callback, timer);

        #endregion

        #region Instance Methods

        private void Update()
        {
            timer -= useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (timer <= 0)
            {
                action?.Invoke();
                DestroySelf();
            }
        }

        private void DestroySelf()
        {
            timerList?.Remove(this);
            if (gameObject) Object.Destroy(gameObject);
        }

        private FunctionTimer(GameObject gameObject, Action action, float timer, string functionName, bool useUnscaledDeltaTime)
        {
            this.gameObject = gameObject;
            this.action = action;
            this.timer = timer;
            this.functionName = functionName;
            this.useUnscaledDeltaTime = useUnscaledDeltaTime;
        }

        #endregion

        #region Helper Classes

        /// A timer that you manually update.
        public class FunctionTimerObject
        {
            private float timer;
            private readonly Action callback;

            public FunctionTimerObject(Action callback, float timer)
            {
                this.callback = callback;
                this.timer = timer;
            }

            /// Updates the timer using Time.deltaTime.
            /// Returns true if the timer has completed.
            public bool Update() => Update(Time.deltaTime);

            /// Updates the timer with a custom deltaTime.
            /// Returns true if the timer has completed.
            public bool Update(float deltaTime)
            {
                timer -= deltaTime;
                if (timer <= 0)
                {
                    callback?.Invoke();
                    return true;
                }

                return false;
            }
        }

        /// Helper MonoBehaviour to hook into Unity's Update loop.
        private class MonoBehaviourHook : MonoBehaviour
        {
            public Action OnUpdate;
            private void Update() => OnUpdate?.Invoke();
        }

        #endregion
    }

    /// Calls function on every Update until it returns true.
    public class FunctionUpdater
    {
        private static List<FunctionUpdater> updaterList;
        private static GameObject initGameObject;

        private readonly GameObject gameObject;
        private readonly string functionName;
        private bool active;
        private readonly Func<bool> updateFunc; // Should return true to signal completion

        private static void TryInitialize()
        {
            if (!initGameObject)
            {
                initGameObject = new GameObject("FunctionUpdater_Global");
                updaterList = new List<FunctionUpdater>();
            }
        }

        /// Creates and starts a FunctionUpdater that calls updateFunc on every Update.
        /// updateFunc should return true when it wants to be destroyed.
        public static FunctionUpdater Create(Func<bool> updateFunc, string functionName = "", bool active = true, bool stopAllWithSameName = false)
        {
            TryInitialize();

            if (stopAllWithSameName) StopAllUpdatersWithName(functionName);

            GameObject obj = new GameObject("FunctionUpdater Object " + functionName, typeof(MonoBehaviourHook));
            FunctionUpdater functionUpdater = new FunctionUpdater(obj, updateFunc, functionName, active);
            obj.GetComponent<MonoBehaviourHook>().OnUpdate = functionUpdater.Update;
            obj.hideFlags = HideFlags.HideInHierarchy;

            updaterList.Add(functionUpdater);
            return functionUpdater;
        }

        private static void RemoveUpdater(FunctionUpdater funcUpdater)
        {
            TryInitialize();
            updaterList.Remove(funcUpdater);
        }

        public static void DestroyUpdater(FunctionUpdater funcUpdater)
        {
            TryInitialize();
            funcUpdater?.DestroySelf();
        }

        public static void StopUpdaterWithName(string functionName)
        {
            TryInitialize();
            foreach (var t in updaterList.Where(t => t.functionName == functionName))
            {
                t.DestroySelf();
                return;
            }
        }

        public static void StopAllUpdatersWithName(string functionName)
        {
            TryInitialize();
            for (int i = 0; i < updaterList.Count; i++)
            {
                if (updaterList[i].functionName == functionName)
                {
                    updaterList[i].DestroySelf();
                    i--;
                }
            }
        }

        public FunctionUpdater(GameObject gameObject, Func<bool> updateFunc, string functionName, bool active)
        {
            this.gameObject = gameObject;
            this.updateFunc = updateFunc;
            this.functionName = functionName;
            this.active = active;
        }

        public void Pause() => active = false;
        public void Resume() => active = true;

        private void Update()
        {
            if (!active) return;
            if (updateFunc()) DestroySelf();
        }

        public void DestroySelf()
        {
            RemoveUpdater(this);
            if (gameObject) Object.Destroy(gameObject);
        }

        /// Helper MonoBehaviour to hook into Unity's Update loop.
        private class MonoBehaviourHook : MonoBehaviour
        {
            public Action OnUpdate;
            private void Update() => OnUpdate?.Invoke();
        }
    }

    /// Executes a Function periodically.
    public class FunctionPeriodic
    {
        private static List<FunctionPeriodic> funcList; // Holds a reference to all active timers
        private static GameObject initGameObject; // Global game object used for initializing class, is destroyed on scene change

        private static void InitIfNeeded()
        {
            if (!initGameObject)
            {
                initGameObject = new GameObject("FunctionPeriodic_Global");
                funcList = new List<FunctionPeriodic>();
            }
        }

        /// Persist through scene loads
        public static FunctionPeriodic Create_Global(Action action, Func<bool> testDestroy, float timer)
        {
            FunctionPeriodic functionPeriodic = Create(action, testDestroy, timer, "", false, false, false);
            Object.DontDestroyOnLoad(functionPeriodic.gameObject);
            return functionPeriodic;
        }

        // Trigger [action] every [timer], execute [testDestroy] after triggering action, destroy if returns true
        public static FunctionPeriodic Create(Action action, Func<bool> testDestroy, float timer)
        {
            return Create(action, testDestroy, timer, "", false);
        }

        public static FunctionPeriodic Create(Action action, float timer)
        {
            return Create(action, null, timer, "", false, false, false);
        }

        public static FunctionPeriodic Create(Action action, float timer, string functionName)
        {
            return Create(action, null, timer, functionName, false, false, false);
        }

        public static FunctionPeriodic Create(Action callback, Func<bool> testDestroy, float timer, string functionName, bool stopAllWithSameName)
        {
            return Create(callback, testDestroy, timer, functionName, false, false, stopAllWithSameName);
        }

        public static FunctionPeriodic Create(Action action, Func<bool> testDestroy, float timer, string functionName, bool useUnscaledDeltaTime, bool triggerImmediately, bool stopAllWithSameName)
        {
            InitIfNeeded();

            if (stopAllWithSameName)
            {
                StopAllFunc(functionName);
            }

            GameObject gameObject = new GameObject("FunctionPeriodic Object " + functionName, typeof(MonoBehaviourHook));
            FunctionPeriodic functionPeriodic = new FunctionPeriodic(gameObject, action, timer, testDestroy, functionName, useUnscaledDeltaTime);
            gameObject.GetComponent<MonoBehaviourHook>().OnUpdate = functionPeriodic.Update;

            funcList.Add(functionPeriodic);

            if (triggerImmediately) action();

            return functionPeriodic;
        }

        public static void RemoveTimer(FunctionPeriodic funcTimer)
        {
            InitIfNeeded();
            funcList.Remove(funcTimer);
        }

        public static void StopTimer(string _name)
        {
            InitIfNeeded();
            foreach (var t in funcList.Where(t => t.functionName == _name))
            {
                t.DestroySelf();
                return;
            }
        }

        public static void StopAllFunc(string _name)
        {
            InitIfNeeded();

            for (int i = 0; i < funcList.Count; i++)
            {
                if (funcList[i].functionName == _name)
                {
                    funcList[i].DestroySelf();
                    i--;
                }
            }
        }

        public static bool IsFuncActive(string name)
        {
            InitIfNeeded();
            return funcList.Any(t => t.functionName == name);
        }

        private readonly GameObject gameObject;
        private float timer;
        private float baseTimer;
        private readonly bool useUnscaledDeltaTime;
        private readonly string functionName;
        public readonly Action action;
        public readonly Func<bool> testDestroy;

        private FunctionPeriodic(GameObject gameObject, Action action, float timer, Func<bool> testDestroy, string functionName, bool useUnscaledDeltaTime)
        {
            this.gameObject = gameObject;
            this.action = action;
            this.timer = timer;
            this.testDestroy = testDestroy;
            this.functionName = functionName;
            this.useUnscaledDeltaTime = useUnscaledDeltaTime;
            baseTimer = timer;
        }

        public void SkipTimerTo(float timer)
        {
            this.timer = timer;
        }

        public void SetBaseTimer(float baseTimer)
        {
            this.baseTimer = baseTimer;
        }

        public float GetBaseTimer()
        {
            return baseTimer;
        }

        private void Update()
        {
            if (useUnscaledDeltaTime)
            {
                timer -= Time.unscaledDeltaTime;
            }
            else
            {
                timer -= Time.deltaTime;
            }

            if (timer <= 0)
            {
                action();
                if (testDestroy != null && testDestroy())
                {
                    //Destroy
                    DestroySelf();
                }
                else
                {
                    //Repeat
                    timer += baseTimer;
                }
            }
        }

        public void DestroySelf()
        {
            RemoveTimer(this);
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }
        }

        /// Class to hook Actions into MonoBehaviour
        private class MonoBehaviourHook : MonoBehaviour
        {
            public Action OnUpdate;
            private void Update() => OnUpdate?.Invoke();
        }
    }

    /// Creates mesh in scene.
    public class MeshGenerator
    {
        private const int sortingOrderDefault = 5000;

        public readonly GameObject gameObject;
        public readonly Transform transform;
        private readonly Material material;
        private Vector2[] uv;
        private readonly Mesh mesh;

        public static MeshGenerator CreateEmpty(Vector3 position, float eulerZ, Material material, int sortingOrderOffset = 0)
        {
            return new MeshGenerator(null, position, Vector3.one, eulerZ, material, Array.Empty<Vector3>(), Array.Empty<Vector2>(), Array.Empty<int>(), sortingOrderOffset);
        }

        public static MeshGenerator Create(Vector3 position, float eulerZ, Material material, Vector3[] vertices, Vector2[] uv, int[] triangles, int sortingOrderOffset = 0)
        {
            return new MeshGenerator(null, position, Vector3.one, eulerZ, material, vertices, uv, triangles, sortingOrderOffset);
        }

        public static MeshGenerator Create(Vector3 position, float eulerZ, float meshWidth, float meshHeight, Material material, UVCoords uvCoords, int sortingOrderOffset = 0)
        {
            return new MeshGenerator(null, position, Vector3.one, eulerZ, meshWidth, meshHeight, material, uvCoords, sortingOrderOffset);
        }

        public static MeshGenerator Create(Vector3 lowerLeftCorner, float width, float height, Material material, UVCoords uvCoords, int sortingOrderOffset = 0)
        {
            return Create(lowerLeftCorner, lowerLeftCorner + new Vector3(width, height), material, uvCoords, sortingOrderOffset);
        }

        public static MeshGenerator Create(Vector3 lowerLeftCorner, Vector3 upperRightCorner, Material material, UVCoords uvCoords, int sortingOrderOffset = 0)
        {
            float width = upperRightCorner.x - lowerLeftCorner.x;
            float height = upperRightCorner.y - lowerLeftCorner.y;
            Vector3 localScale = upperRightCorner - lowerLeftCorner;
            Vector3 position = lowerLeftCorner + localScale * .5f;
            return new MeshGenerator(null, position, Vector3.one, 0f, width, height, material, uvCoords, sortingOrderOffset);
        }


        private static int GetSortingOrder(Vector3 position, int offset, int baseSortingOrder = sortingOrderDefault)
        {
            return (int)(baseSortingOrder - position.y) + offset;
        }


        public class UVCoords
        {
            public readonly int x;
            public readonly int y;
            public readonly int width;
            public readonly int height;

            public UVCoords(int x, int y, int width, int height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }

        public MeshGenerator(Transform parent, Vector3 localPosition, Vector3 localScale, float eulerZ, float meshWidth, float meshHeight, Material material, UVCoords uvCoords, int sortingOrderOffset)
        {
            this.material = material;

            var vertices1 = new Vector3[4];
            uv = new Vector2[4];
            var triangles1 = new int[6];

            /* 0,1
             * 1,1
             * 0,0
             * 1,0
             */
            float meshWidthHalf = meshWidth / 2f;
            float meshHeightHalf = meshHeight / 2f;

            vertices1[0] = new Vector3(-meshWidthHalf, meshHeightHalf);
            vertices1[1] = new Vector3(meshWidthHalf, meshHeightHalf);
            vertices1[2] = new Vector3(-meshWidthHalf, -meshHeightHalf);
            vertices1[3] = new Vector3(meshWidthHalf, -meshHeightHalf);

            uvCoords ??= new UVCoords(0, 0, material.mainTexture.width, material.mainTexture.height);

            var uvArray = GetUVRectangleFromPixels(uvCoords.x, uvCoords.y, uvCoords.width, uvCoords.height, material.mainTexture.width, material.mainTexture.height);

            ApplyUVToUVArray(uvArray, ref uv);

            triangles1[0] = 0;
            triangles1[1] = 1;
            triangles1[2] = 2;
            triangles1[3] = 2;
            triangles1[4] = 1;
            triangles1[5] = 3;

            mesh = new Mesh
            {
                vertices = vertices1,
                uv = uv,
                triangles = triangles1
            };

            gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer))
            {
                transform =
                {
                    parent = parent,
                    localPosition = localPosition,
                    localScale = localScale,
                    localEulerAngles = new Vector3(0, 0, eulerZ)
                }
            };

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;

            transform = gameObject.transform;

            SetSortingOrderOffset(sortingOrderOffset);
        }

        public MeshGenerator(Transform parent, Vector3 localPosition, Vector3 localScale, float eulerZ, Material material, Vector3[] vertices, Vector2[] uv, int[] triangles, int sortingOrderOffset)
        {
            this.material = material;
            this.uv = uv;

            mesh = new Mesh
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles
            };

            gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer))
            {
                transform =
                {
                    parent = parent,
                    localPosition = localPosition,
                    localScale = localScale,
                    localEulerAngles = new Vector3(0, 0, eulerZ)
                }
            };

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;

            transform = gameObject.transform;

            SetSortingOrderOffset(sortingOrderOffset);
        }

        private Vector2 ConvertPixelsToUVCoordinates(int x, int y, int textureWidth, int textureHeight)
        {
            return new Vector2((float)x / textureWidth, (float)y / textureHeight);
        }

        private Vector2[] GetUVRectangleFromPixels(int x, int y, int width, int height, int textureWidth, int textureHeight)
        {
            /* 0, 1
             * 1, 1
             * 0, 0
             * 1, 0
             * */
            return new[]
            {
                ConvertPixelsToUVCoordinates(x, y + height, textureWidth, textureHeight),
                ConvertPixelsToUVCoordinates(x + width, y + height, textureWidth, textureHeight),
                ConvertPixelsToUVCoordinates(x, y, textureWidth, textureHeight),
                ConvertPixelsToUVCoordinates(x + width, y, textureWidth, textureHeight)
            };
        }

        private void ApplyUVToUVArray(Vector2[] uv, ref Vector2[] mainUV)
        {
            if (uv == null || uv.Length < 4 || mainUV == null || mainUV.Length < 4) throw new Exception();
            mainUV[0] = uv[0];
            mainUV[1] = uv[1];
            mainUV[2] = uv[2];
            mainUV[3] = uv[3];
        }

        public void SetUVCoords(UVCoords uvCoords)
        {
            var uvArray = GetUVRectangleFromPixels(uvCoords.x, uvCoords.y, uvCoords.width, uvCoords.height, material.mainTexture.width, material.mainTexture.height);
            ApplyUVToUVArray(uvArray, ref uv);
            mesh.uv = uv;
        }

        public void SetSortingOrderOffset(int sortingOrderOffset)
        {
            SetSortingOrder(GetSortingOrder(gameObject.transform.position, sortingOrderOffset));
        }

        public void SetSortingOrder(int sortingOrder)
        {
            gameObject.GetComponent<Renderer>().sortingOrder = sortingOrder;
        }

        public void SetLocalScale(Vector3 localScale)
        {
            transform.localScale = localScale;
        }

        public void SetPosition(Vector3 localPosition)
        {
            transform.localPosition = localPosition;
        }

        public void AddPosition(Vector3 addPosition)
        {
            transform.localPosition += addPosition;
        }

        public Vector3 GetPosition()
        {
            return transform.localPosition;
        }

        public int GetSortingOrder()
        {
            return gameObject.GetComponent<Renderer>().sortingOrder;
        }

        public Mesh GetMesh()
        {
            return mesh;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void DestroySelf()
        {
            Object.Destroy(gameObject);
        }

        public static void CreateMesh()
        {
        }
    }

    #endregion
}