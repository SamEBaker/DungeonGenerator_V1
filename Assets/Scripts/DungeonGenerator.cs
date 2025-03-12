using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

//https://www.youtube.com/watch?v=fbGtrZlOKDM&t=290s&ab_channel=V0LAT1LE_
public class DungeonGenerator : MonoBehaviour
{
    //add function to check special rooms to check if amt of vault rooms != to key rooms change one of the generated rooms to a key room
    //keys and vaults have enum to change type manually after generation
   public static DungeonGenerator Instance { get; private set; }
    [SerializeField]
    private List<GameObject> rooms;
    [SerializeField]
    private List<GameObject> specialRooms;
    [SerializeField]
    private List<GameObject> hallways;
    //[SerializeField]
    public int numOfRooms = 12;
    [SerializeField]
    private LayerMask roomsLayerMask;
    [SerializeField]
    private GameObject door;
    [SerializeField]
    private GameObject entrance;
    private List<DungeonPart> generatedRooms;
    private bool isGenerated = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        generatedRooms = new List<DungeonPart>();
    }

    public void StartGeneration()
    {
        Generate();
        //BalanceSpecialRooms(){if keyRoomsGenerated != VaultRoomsGenerated addKeyRoom(){replace random generated room or try add special room?} otherwise add vaultRoom otherwise break;
        FillEmptyEntrances();
        isGenerated = true;
    }
    private void Generate()
    {
        for(int i = 0; i < numOfRooms; i++)
        {
            if(generatedRooms.Count < 1)
            {
                GameObject generatedRoom = Instantiate(entrance, transform.position, transform.rotation);


                if(generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                {
                    generatedRooms.Add(dungeonPart);
                }
            }
            else
            {
                bool placeHallway = Random.Range(0f, 1f) > 0.2f;
                DungeonPart randGeneratedRoom = null;
                Transform room1EntryPt = null;
                int totalRetries = 100;
                int retryindex = 0;
                while(randGeneratedRoom == null && retryindex < totalRetries)
                {
                    int randomLinkIndex = Random.Range(0, generatedRooms.Count);
                    DungeonPart roomTest = generatedRooms[randomLinkIndex];
                    if(roomTest.HasAvailableEntryPoint(out room1EntryPt))
                    {
                        randGeneratedRoom = roomTest;
                        break;
                    }
                    retryindex++;
                }

                GameObject doorToAlign = Instantiate(door, transform.position, transform.rotation); 

                // 50/50 chance to place hallway
                if (placeHallway)
                {
                    int randIndex = Random.Range(0, hallways.Count);
                    GameObject generatedHallway = Instantiate(hallways[randIndex], transform.position, transform.rotation);
                    if(generatedHallway.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntryPoint(out Transform room2EntryPt))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1EntryPt.transform.position;
                            doorToAlign.transform.rotation = room1EntryPt.transform.rotation;
                            AlignRooms(generatedHallway.transform, room1EntryPt, room2EntryPt);

                            if (HandleOverlap(dungeonPart))
                            {
                                //undo everything and retry
                                dungeonPart.UnuseEntryPoint(room2EntryPt);
                                randGeneratedRoom.UnuseEntryPoint(room1EntryPt);
                                RetryPlacement(generatedHallway, doorToAlign);
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    GameObject generatedRoom;
                    if(specialRooms.Count > 0)
                    {
                        bool placeSpecialRoom = Random.Range(0f, 1f) > 0.7f;
                        //20% chance to spawn special room

                        if (placeSpecialRoom)
                        {
                            int randomIndex = Random.Range(0, specialRooms.Count);
                            generatedRoom = Instantiate(specialRooms[randomIndex], transform.position, transform.rotation);
                            //CheckRoomType() { generatedRoom. name == keyRoom, keyRoomsGenerated++; same with Vault Rooms}
                        }
                        else
                        {
                            int randomIndex = Random.Range(0, rooms.Count);
                            generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                        }
                        
                    }
                    else
                    {
                        int randomIndex = Random.Range(0, rooms.Count);
                        generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                    }
                    if(generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if(dungeonPart.HasAvailableEntryPoint(out Transform room2EntryPt))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1EntryPt.transform.position;
                            doorToAlign.transform.rotation = room1EntryPt.transform.rotation;
                            AlignRooms(generatedRoom.transform, room1EntryPt, room2EntryPt);

                            if (HandleOverlap(dungeonPart))
                            {
                                //undo everything and retry
                                dungeonPart.UnuseEntryPoint(room2EntryPt);
                                randGeneratedRoom.UnuseEntryPoint(room1EntryPt);
                                RetryPlacement(generatedRoom, doorToAlign);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    private void RetryPlacement(GameObject roomToPlace, GameObject doorToPlace)
    {
        DungeonPart randGeneratedRoom = null;
        Transform room1EntryPt = null;
        int totalRetries = 100;
        int retryIndex = 0;

        while (randGeneratedRoom == null && retryIndex < totalRetries)
        {
            int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count - 1);
            DungeonPart roomTest = generatedRooms[randomLinkRoomIndex];
            if (roomTest.HasAvailableEntryPoint(out room1EntryPt))
            {
                randGeneratedRoom = roomTest;
                break;
            }
            retryIndex++;
        }
        if(roomToPlace.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
        {
            if(dungeonPart.HasAvailableEntryPoint(out Transform room2EntryPt))
            {
                doorToPlace.transform.position = room1EntryPt.transform.position;
                doorToPlace.transform.rotation = room1EntryPt.transform.rotation;
                AlignRooms(roomToPlace.transform, room1EntryPt, room2EntryPt);

                if (HandleOverlap(dungeonPart))
                {
                    dungeonPart.UnuseEntryPoint(room2EntryPt);
                    randGeneratedRoom.UnuseEntryPoint(room1EntryPt);
                    RetryPlacement(roomToPlace, doorToPlace);
                }
            }
        }
    }


    private void FillEmptyEntrances()
    {
        generatedRooms.ForEach(room => room.FillEmptyDoors());
    }

    private bool HandleOverlap(DungeonPart dungeonPart)
    {
        bool isOverlapped = false;

        //creates collider and outputs colliders found inside of it
        Collider[] hits = Physics.OverlapBox(dungeonPart.collider.bounds.center, dungeonPart.collider.bounds.size / 2, Quaternion.identity, roomsLayerMask);

        foreach(Collider hit in hits)
        {
            //if it hits itself ignore
            if (hit == dungeonPart.collider) continue;
            if(hit != dungeonPart.collider)
            {
                isOverlapped = true;
                break;
            }
        }
        return isOverlapped;
    }
    private void AlignRooms(Transform room2, Transform room1Entry, Transform room2Entry)
    {
        //get angle btx each room entry pt to connect
        float angle = Vector3.Angle(room1Entry.forward, room2Entry.forward);
        room2.TransformPoint(room2Entry.position);
        room2.eulerAngles = new Vector3(room2.eulerAngles.x, room2.eulerAngles.y + angle, room2.eulerAngles.z);
        //rooms to face each other
        Vector3 offset = room1Entry.position - room2Entry.position;
        room2.position += offset;
        //move to rooms to connect and have no gap
        Physics.SyncTransforms();
        //helps update colliders position when moving objects
    }
    public List<DungeonPart> GetGeneratedRooms() => generatedRooms;

    public bool IsGenerated() => isGenerated;
}
