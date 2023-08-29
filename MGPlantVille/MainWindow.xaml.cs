using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using MGPlantVille.Models;

/// <summary>
/// PlantVille Game
/// By: Miguel Gutiérrez
/// </summary>
namespace MGPlantVille
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int money = 100;
        public int landPlots = 15;
        public int marketFee = 10;
        private const string filePath = "player_data.txt";

        private static List<Seed> seed_list = new List<Seed>() {
            new Seed("strawberry", 2, 8, new TimeSpan(0, 0, 30)),
            new Seed("spinach", 5, 21, new TimeSpan(0, 1, 0)),
            new Seed("pears", 3, 20, new TimeSpan(0, 3, 0))
        };
        private List<Plant> garden = new List<Plant>();
        private List<Plant> inventory = new List<Plant>();
        
        public MainWindow()
        {
            // Calling loaders
            InitializeComponent();
            LoadData();
            LoadGardenData();
        }
        /// <summary>
        /// Switching grids visibility after Emporium button clicked
        /// </summary>
        private void btnSeedsEmporium_Click(object sender, RoutedEventArgs e)
        {
            gridGarden.Visibility = Visibility.Hidden;
            gridSeedsEmporium.Visibility = Visibility.Visible;
            gridInventory.Visibility = Visibility.Hidden;
            LoadSeedsEmporiumData();
        }
        /// <summary>
        /// Switching grids visibility after Garden button clicked
        /// </summary>
        private void btnGarden_Click(object sender, RoutedEventArgs e)
        {
            gridGarden.Visibility = Visibility.Visible;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            LoadGardenData();
        }
        /// <summary>
        /// Switching grids visibility after Inventory button clicked
        /// </summary>
        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridGarden.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Visible;
            LoadInventoryData();
        }
        /// <summary>
        /// Load Data From the file if exists.
        /// </summary>
        private void LoadData()
        {
            if (File.Exists(filePath))
            {
                string jsonContent;
                using (StreamReader streamReader = new StreamReader(filePath))
                {
                    // Read FILE content
                    jsonContent = streamReader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(jsonContent))
                    return;

                // Deserialize the JSON content into a dictionary
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

                // Validates data to set it properly into lists
                if (dictionary.ContainsKey("garden"))
                {
                    string gardenJson = dictionary["garden"].ToString();
                    garden = JsonConvert.DeserializeObject<List<Plant>>(gardenJson);
                }

                if (dictionary.ContainsKey("inventory"))
                {
                    string inventoryJson = dictionary["inventory"].ToString();
                    inventory = JsonConvert.DeserializeObject<List<Plant>>(inventoryJson);
                }

                if (dictionary.ContainsKey("money"))
                {
                    money = Convert.ToInt32(dictionary["money"]);
                }
            }
            // Update the status bar after loading the data
            UpdateStatus();
        }

        /// <summary>
        /// Save Data to the file
        /// </summary>
        private void SaveData()
        {
            // Create a Dictionary to store the game data
            // Stores the Garden list, Inventory list and Money value
            var data = new Dictionary<string, object>()
            {
                { "garden", garden },
                { "inventory", inventory },
                { "money", money }
            };

            // Convert the data dictionary to JSON format and write it to a file
            var jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Update Status bar info with Money and Land Plots data
        /// </summary>
        private void UpdateStatus()
        {
            lblStatus.Content = $"Money: ${money}\nLand: {landPlots - garden.Count}";
        }

        /// <summary>
        /// Loads Garden data (plants) into a listbox if exists
        /// </summary>
        private void LoadGardenData()
        {
            listBoxGarden.Items.Clear();
            if (garden.Count < 1)
            {
                listBoxGarden.Items.Add("No plants in the garden.");
            }
            else
            {
                foreach (Plant plant in garden)
                {
                    string status = CheckPlantStatus(plant);
                    listBoxGarden.Items.Add($"{plant.Seed.Name} ({status})");
                }
            }
            UpdateStatus();
        }

        /// <summary>
        /// Loads Inventory data into a listbox with the already harvested plants
        /// </summary>
        private void LoadInventoryData()
        {
            listBoxInventory.Items.Clear();
            foreach (var plant in inventory)
            {
                listBoxInventory.Items.Add($"{plant.Seed.Name} ${plant.Seed.HarvestPrice}");
            }

            if (inventory.Count == 0)
            {
                listBoxInventory.Items.Add("No fruits or vegetables harvested.");
            }

            UpdateStatus();
        }
        /// <summary>
        /// Loads the Seeds Emporioum data into a listbox with the list of available Seeds
        /// </summary>
        private void LoadSeedsEmporiumData()
        {
            listBoxSeedsEmporium.Items.Clear();
            foreach (Seed seed in seed_list)
                listBoxSeedsEmporium.Items.Add(string.Format("{0} ${1}", seed.Name, seed.SeedPrice));
            UpdateStatus();
        }
        /// <summary>
        /// Validates if a plant is ready to be harvested,
        /// how much more time it will to be harvested
        /// and if is not spoiled 
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        private string CheckPlantStatus(Plant plant)
        {
            TimeSpan timeSincePlanting = DateTime.Now.Subtract(plant.HarvestTime);
            if (timeSincePlanting >= plant.Seed.HarvestDuration)
            {
                if (!plant.IsSpoiled)
                {
                    return "ready to harvest";
                }
                else
                {
                    return "spoiled";
                }
            }
            else
            {
                TimeSpan timeRemaining = plant.Seed.HarvestDuration.Subtract(timeSincePlanting);
                int secondsLeft = (int)timeRemaining.TotalSeconds;
                return $"{secondsLeft} seconds left";
            }
        }

        /// <summary>
        /// Handles the double click on the listbox items of the Seeds Emporium listbox
        /// Performs the sale process of the seeds and assign them to the available land plots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxSeedsEmporium_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedIndex = listBoxSeedsEmporium.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < seed_list.Count)
            {
                var selectedSeed = seed_list[selectedIndex];
                if (selectedSeed.SeedPrice > money)
                {
                    MessageBox.Show("You don't have enough money.");
                }
                else if (landPlots - garden.Count < 1)
                {
                    MessageBox.Show("You don't have enough land to plant another crop.");
                }
                else
                {
                    money -= selectedSeed.SeedPrice;
                    garden.Add(new Plant(selectedSeed));
                    MessageBox.Show($"You purchased {selectedSeed.Name}");
                    ValidateMoney();
                    UpdateStatus();
                }
            }
        }
        /// <summary>
        /// Handles the double click on the listbox items of the plants on the Garden 
        /// Validates the plant status and performs the harvest process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxGarden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = listBoxGarden.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < garden.Count)
            {
                Plant selectedPlant = garden[selectedIndex];
                string plantStatus = CheckPlantStatus(selectedPlant);
                if (plantStatus == "ready to harvest")
                {
                    inventory.Add(selectedPlant);
                    garden.RemoveAt(selectedIndex);
                    MessageBox.Show($"{selectedPlant.Seed.Name} harvested.");
                    
                }
                else if (plantStatus == "spoiled")
                {
                    if (MessageBox.Show($"{ selectedPlant.Seed.Name} is spoiled and cannot be harvested. Do you want to dispose it?", "Your plant is spoiled!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            return;
                        }
                        garden.RemoveAt(selectedIndex);
                }
                else
                {
                    MessageBox.Show($"{selectedPlant.Seed.Name} is not ready to be harvested yet. {plantStatus}");
                }
                LoadGardenData();
            }
        }
        /// <summary>
        /// Handles the click on the Harvest button
        /// Validates each of the plants status and performs the harvest process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
  
        private void btnHarvestAll_Click(object sender, RoutedEventArgs e)
        {
            int harvestedCount = 0;
            List<Plant> harvestedPlants = new List<Plant>();
            bool hasSpoiledPlants = false;

            for (int i = garden.Count - 1; i >= 0; i--)
            {
                Plant plant = garden[i];
                string plantStatus = CheckPlantStatus(plant);

                if (plantStatus == "ready to harvest")
                {
                    harvestedPlants.Add(plant);
                    garden.RemoveAt(i);
                    harvestedCount++;
                }
                else if (plantStatus == "spoiled")
                {
                    hasSpoiledPlants = true;
                }
            }

            if (hasSpoiledPlants)
            {
                MessageBoxResult result = MessageBox.Show("One or more of your plants are spoiled. Do you want to dispose of them?", "Spoiled Plant Found!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    garden.RemoveAll(plant => CheckPlantStatus(plant) == "spoiled");
                    MessageBox.Show("Spoiled plants disposed.");
                }
            }

            if (harvestedCount > 0)
            {
                inventory.AddRange(harvestedPlants);
                MessageBox.Show($"Harvested {harvestedCount} plants.");
                LoadGardenData();
            }
            else
            {
                MessageBox.Show("Nothing to harvest.");
            }
        }
        /// <summary>
        /// Handles the click on the Sell in Farmer's Market button
        /// Deducts the $10 fee of the selling inventary process
        /// Calculates each of the plants value and performs the selling process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSellInventory_Click(object sender, RoutedEventArgs e)
        {
            if (inventory.Count == 0 && 
                MessageBox.Show("The Farmer's Market fee is $10. Do you want to go without any inventory?", "You are about to lose money!!!", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            var income = inventory.Sum(plant => plant.Seed.HarvestPrice);
            var netProfit = income - marketFee;
            var sellingResult = $"Made ${income} from your plants - ${marketFee} Farmer's Market fee \n Net profit: {netProfit}";
            if (income <= 0) {
                sellingResult = $"Lost -${marketFee}";
            }
            money += netProfit;
            inventory.Clear();
            MessageBox.Show($"Cleared inventory. You {sellingResult}", "Cleared inventory");
            LoadInventoryData();
            // Validating that the player's money is not negative
            ValidateMoney();

            
        }
        /// <summary>
        /// Validates if user has enough Money to keep playing
        /// If not restarts the game
        /// </summary>
        private void ValidateMoney()
        {
            if (money < 0)
            {
                MessageBoxResult result = MessageBox.Show("Oh no, you've gone bankrupt! Do you want to play again?", "Game Over!", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Restart the game by resetting the money and land plots
                    money = 100;
                    landPlots = 15;

                    // Clear the data file
                    ClearData();

                    // Reload the necessary data or reset the game state
                    garden.Clear();
                    inventory.Clear();

                    // Update the UI
                    UpdateStatus();
                    LoadGardenData();
                }
                else
                {
                    money = 100;
                    landPlots = 15;
                    // Exit the game and clear the data file
                    ClearData();
                    Close();
                }
            }
        }
        /// <summary>
        /// Clear all data from the file
        /// </summary>
        private void ClearData()
        {
            var jsonData = "";
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Handles the Closing event of the App Window
        /// Saves data into the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveData();

        }
        /// <summary>
        /// Handles the event on the Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
