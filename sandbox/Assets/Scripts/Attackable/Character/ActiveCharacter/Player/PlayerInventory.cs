using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Character2D
{
    public class PlayerInventory : MonoBehaviour
    {
        public GameObject pickupPrefab;
        public List<InventoryItem> items; //items held by the character

        public static event Action<LevelItemTuple, bool, string> OnLooseItemChanged;
        public static event Action<ItemTuple> OnInventoryItemChanged;

        protected FMOD.Studio.EventInstance pickupNoise;
        protected FMOD.Studio.EventInstance dropNoise;
        protected FMOD.Studio.EventInstance destroyNoise;

        protected FMOD.Studio.EventInstance equipNoise;

        //used for initialization
        protected void Start()
        {
            items = new List<InventoryItem>();
            InitializeInventory();

            pickupNoise = FMODUnity.RuntimeManager.CreateInstance("event:/Menus_Inventory/pickup_item");
            pickupNoise.setVolume(PlayerPrefs.GetFloat("SfxVolume") * PlayerPrefs.GetFloat("MasterVolume"));

            dropNoise = FMODUnity.RuntimeManager.CreateInstance("event:/Menus_Inventory/drop_item");
            dropNoise.setVolume(PlayerPrefs.GetFloat("SfxVolume") * PlayerPrefs.GetFloat("MasterVolume"));

            destroyNoise = FMODUnity.RuntimeManager.CreateInstance("event:/Menus_Inventory/destroy_item");
            destroyNoise.setVolume(PlayerPrefs.GetFloat("SfxVolume") * PlayerPrefs.GetFloat("MasterVolume"));

            equipNoise = FMODUnity.RuntimeManager.CreateInstance("event:/Menus_Inventory/equip_armor_weapon");
            equipNoise.setVolume(PlayerPrefs.GetFloat("SfxVolume") * PlayerPrefs.GetFloat("MasterVolume"));
        }

        private void InitializeInventory()
        {
            var savedItems = new List<ItemTuple>();
            savedItems = GameData.data.saveData.ReadPlayerItems();
            foreach (ItemTuple item in savedItems)
            {
                for (int index = 0; index < item.quantity; index++)
                {
                    AddItem(item.name, true);
                }
            }
        }

        public void AddItem(string itemName, bool init = false, bool equip = false)
        {
            bool found = false;
            var itemTuple = new ItemTuple();
            itemTuple.name = itemName;
            itemTuple.quantity = 0;

            foreach (var item in items)
            {
                if (item.item == GameData.data.itemData.itemDictionary[itemName])
                {
                    item.quantity++;
                    itemTuple.quantity = item.quantity;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var itemToAdd = new InventoryItem
                {
                    item = GameData.data.itemData.itemDictionary[itemName],
                    quantity = 1
                };
                itemTuple.quantity = itemToAdd.quantity;
                items.Add(itemToAdd);
            }

            if (!init && !equip)
            {
                EventDisplay.instance.AddEvent("Received " + itemName + ".");
                OnInventoryItemChanged(itemTuple);
                pickupNoise.start();
            }
            else if (!init && equip)
            {
                EventDisplay.instance.AddEvent("Unequipped " + itemName + ".");
                OnInventoryItemChanged(itemTuple);
            }
        }

        public void CollectPickup(Pickup pickup)
        {
            var levelItem = new LevelItemTuple();
            levelItem.id = pickup.id;
            OnLooseItemChanged(levelItem, false, SceneManager.GetActiveScene().name);
            AddItem(pickup.name);
        }

        public void RemoveItem(InventoryItem itemToRemove, bool removeAll, bool drop,
            bool isTransaction = false, bool equip = false, bool use = false, bool give = false)
        {
            var amountToDrop = 0;
            int amountLeft = 0;
            if (removeAll)
            {
                amountToDrop = itemToRemove.quantity;
                amountLeft = 0;
                items.Remove(itemToRemove);
            }
            else
            {
                amountToDrop = 1;
                itemToRemove.quantity--;
                amountLeft = itemToRemove.quantity;
                if (itemToRemove.quantity <= 0)
                {
                    items.Remove(itemToRemove);
                }
            }

            var itemTuple = new ItemTuple();
            itemTuple.name = itemToRemove.item.name;
            itemTuple.quantity = amountLeft;
            OnInventoryItemChanged(itemTuple);

            if (isTransaction)
            {
                EventDisplay.instance.AddEvent("Sold " + amountToDrop + " " + itemToRemove.item.name + "(s).");
                return;
            }

            if (equip)
            {
                equipNoise.start();
                EventDisplay.instance.AddEvent("Equipped " + itemToRemove.item.name + ".");
                return;
            }

            if (use)
            {
                destroyNoise.start();
                EventDisplay.instance.AddEvent("Used " + itemToRemove.item.name + ".");
                return;
            }

            if (give)
            {
                EventDisplay.instance.AddEvent("Gave " + itemToRemove.item.name + ".");
                return;
            }

            if (!drop)
            {
                destroyNoise.start();
                EventDisplay.instance.AddEvent("Destroyed " + amountToDrop + " " + itemToRemove.item.name + "(s).");
                return;
            }

            dropNoise.start();

            EventDisplay.instance.AddEvent("Dropped " + amountToDrop + " " + itemToRemove.item.name + "(s).");
            for (var i = 0; i < amountToDrop; i++)
            {
                InstantiateItem(itemToRemove.item, transform.position);
            }

        }

        public void InstantiateItem(Item item, Vector3 pos, bool write = true, int id = 0)
        {
            //instantiate a prefab for the pickup
            var newPickup = Instantiate(pickupPrefab) as GameObject;
            var pickup = newPickup.GetComponent<Pickup>();
            var rend = newPickup.GetComponentInChildren<SpriteRenderer>();

            //set the properties for the pickup
            pickup.name = item.name;
            pickup.typeOfInteractable = Interactable.Types.Pickup;
            pickup.SetType();
            rend.sprite = item.sprite;
            rend.material = item.material;
            newPickup.transform.position = pos;

            //for some reason Unity does not use full scale for the instantiated object by default
            newPickup.transform.localScale = Vector3.one;

            if (write)
            {
                pickup.GenerateID();
                while (!GameData.data.saveData.ValidateUniqueItemID(pickup.id))
                {
                    pickup.GenerateID();
                }
                var levelItem = new LevelItemTuple();
                levelItem.id = pickup.id;
                levelItem.name = pickup.name;
                levelItem.xLoc = pos.x;
                levelItem.yLoc = pos.y;
                OnLooseItemChanged(levelItem, true, SceneManager.GetActiveScene().name);
            }
            else
            {
                pickup.id = id;
            }
        }

        public bool CheckForItemAndRemove(string itemName, int amount)
        {
            foreach (InventoryItem item in items)
            {
                if (item.item.name == itemName)
                {
                    if (item.quantity >= amount)
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            RemoveItem(item, false, false);
                        }
                        return true;
                    }
                }
            }
            EventDisplay.instance.AddEvent("You do not have enough " + itemName + "s. You need " + amount + ".");
            return false;
        }

        public bool CheckForItem(string itemName, int amount)
        {
            foreach (InventoryItem item in items)
            {
                if (item.item.name == itemName)
                {
                    if (item.quantity >= amount)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public InventoryItem GetInventoryItem(string itemName)
        {
            foreach (InventoryItem item in items)
            {
                if (item.item.name == itemName)
                {
                    return item;
                }
            }
            Debug.LogError("Inventory item not found when trying to access it: " + itemName);
            return null;
        }
    }
}
