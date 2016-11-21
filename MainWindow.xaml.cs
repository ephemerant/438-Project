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
using System.IO;
using System.Windows.Threading;

namespace UNO
{
    public partial class MainWindow : Window
    {
        //------------------------------
        // Variables
        //------------------------------

        public string resourcesPath;
        public string imagesPath;

        public List<Image> menuButtons = new List<Image>();//the list of button images
        public List<Image> arrows = new List<Image>();

        public List<Player> playerList = new List<Player>();

        public Game game = new Game();
        public Lobby lobby = new Lobby();
        public MainMenu menu = new MainMenu();

        Screen currentScreen;

        //------------------------------
        // Functions
        //------------------------------

        public MainWindow()
        {
            InitializeComponent();

            Keyboard.AddKeyUpHandler(this, KeyUpHandler);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            currentScreen.KeyUpHandler(sender, e);
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start at the Main Menu
            currentScreen = menu;
            menu.Load(this);
        }

        public void unloadMenuScreen()
        {
            menu.Unload();
        }

        public void unloadMainScreen()
        {
            game.Unload();
        }

        //load host screen assets
        public void loadHostScreen()
        {
            currentScreen = lobby;
            lobby.Load(this, true);
        }

        public void unloadHostScreen()
        {
            lobby.Unload(true);
        }

        //load join screen assets
        public void loadJoinScreen()
        {
            currentScreen = lobby;
            lobby.Load(this, false);
        }

        public void unloadJoinScreen()
        {
            lobby.Unload(false);
        }

        //Open the main screen
        public void StartMainScreen()
        {
            currentScreen = game;
            game.Load(this);
        }

        public void DrawDeckLeftButtonDown(object sender, MouseEventArgs e)
        {
            currentScreen.DrawDeckLeftButtonDown(sender, e);
        }

        public void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentScreen.CanvasMouseLeftButtonDown(sender, e);
        }

        public void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            currentScreen.CanvasMouseLeftButtonUp(sender, e);
        }

        public void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            currentScreen.CanvasMouseMove(sender, e);
        }

        public void DrawDeckLeftButtonUp(object sender, MouseEventArgs e)
        {
            currentScreen.DrawDeckLeftButtonUp(sender, e);
        }
    }
}
