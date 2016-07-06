using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace TcClone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }
        private ProgressDialog progressBar = new ProgressDialog();
        ObservableCollection<DirectoryEntry> tmpentries = new ObservableCollection<DirectoryEntry>();
        ObservableCollection<DirectoryEntry> entries = new ObservableCollection<DirectoryEntry>();
        ObservableCollection<DirectoryEntry> right_panel_subEntries = new ObservableCollection<DirectoryEntry>();
        ObservableCollection<DirectoryEntry> left_panel_subEntries = new ObservableCollection<DirectoryEntry>();
        DirectoryEntry[] dentry = new DirectoryEntry[2];
        EntryType[] extension = new EntryType[2];
        bool[] rootFolder = { true, true };
        string[] sItem = new string[2];
        int actPanel = 0;
        bool bCancel = false;
        bool overWriteAll = false; //
        bool eztMarKerdeztem = false, useBuiltInCopy = true;
        int entryCount = 0;
        int[] sortInd = { 0, 0 };
        string workedPath = "";

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo s in allDrives)  
            {
                DirectoryEntry d = new DirectoryEntry(s.Name + " (" + s.DriveType + ")", s.Name, "<Driver>", "<DIR>", Directory.GetLastWriteTime(s.Name), PhysicalPath(@"..\..\Images\diskdrive.png"), EntryType.Dir);
                d.drvType = s.DriveType;
                d.IsDrive = true;
                entries.Add(d);
            }

            this.listView1.DataContext = entries;
            this.listView2.DataContext = entries;
            progressBar.ProgressType = !useBuiltInCopy;
            progressBar.ButtonEnabled = useBuiltInCopy;
            progressBar.Cancel += CancelProcess;
            progressBar.Owner = this;
            progressBar.Hide();
        }

        public string PhysicalPath(string filename)
        {
            return System.IO.Path.GetFullPath(filename);
        }

        void getDrives(ListView lv)
        {
            entries.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo s in allDrives)
            {
                DirectoryEntry d = new DirectoryEntry(s.Name + " (" + s.DriveType + ")", s.Name, "<Driver>", "<DIR>", Directory.GetLastWriteTime(s.Name), PhysicalPath(@"..\..\Images\diskdrive.png"), EntryType.Dir);
                d.IsDrive = true;
                d.drvType = s.DriveType;
                entries.Add(d);
            }
            lv.DataContext = entries;
            dentry[actPanel - 1] = entries[0];
            if (actPanel == 1)
                leftSelected.Content = "";
            else
                rightSelected.Content = "";
        }

        void left_ListViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            DirectoryEntry entry = item.DataContext as DirectoryEntry;
            if (item != null && item.IsSelected)
            {
                actPanel = 1;
                listView1.Background = Brushes.LightYellow;
                listView2.Background = Brushes.White;
                sItem[0] = entry.Fullpath;
                extension[0] = entry.Type;
            }
        }

        void left_listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = e.Source as ListViewItem;
            actPanel = 1;
            listView1.Background = Brushes.LightYellow;
            listView2.Background = Brushes.White;
            dentry[0] = item.DataContext as DirectoryEntry;
            if (dentry[0].Type == EntryType.Dir)
            {
                panelRefresh(listView1, left_panel_subEntries, "");
                leftSelected.Content = dentry[actPanel - 1].Fullpath;
                if ((left_panel_subEntries.Count == 1) && dentry[0].IsDrive)
                    System.Windows.MessageBox.Show(dentry[0].Name + " üres, vagy nem olvasható!");
            }
            else
                openFile();
        }

        void right_ListViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            DirectoryEntry entry = item.DataContext as DirectoryEntry;
            if (item != null && item.IsSelected)
            {
                actPanel = 2;
                listView2.Background = Brushes.LightYellow;
                listView1.Background = Brushes.White;
                sItem[1] = entry.Fullpath;
                extension[1] = entry.Type;
            }
        }

        void right_listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = e.Source as ListViewItem;
            actPanel = 2;
            listView2.Background = Brushes.LightYellow;
            listView1.Background = Brushes.White;
            dentry[1] = item.DataContext as DirectoryEntry;
            if (dentry[1].Type == EntryType.Dir)
            {
                panelRefresh(listView2, right_panel_subEntries, "");
                rightSelected.Content = dentry[actPanel - 1].Fullpath;
                if ((right_panel_subEntries.Count == 1) && dentry[1].IsDrive)
                    System.Windows.MessageBox.Show(dentry[1].Name + " üres, vagy nem olvasható!");
            }
            else
                openFile();
        }

        void panelRefresh(ListView lv, ObservableCollection<DirectoryEntry> subEntries, string sPath)
        {
            String path;
            bool upStep = false;
            if (dentry[actPanel - 1] != null)
            {
                sItem[actPanel - 1] = dentry[actPanel - 1].Fullpath;
                extension[actPanel - 1] = dentry[actPanel - 1].Type;
                if ((dentry[actPanel - 1].Type == EntryType.Dir) || (sPath != ""))
                {
                    if (dentry[actPanel - 1].Fullpath == null)
                    {
                        getDrives(lv);
                    }
                    else
                    {
                        string lastPath, firstPath = "";
                        if (sPath != "")
                        {
                            path = sPath;
                            lastPath = path.Substring(0, path.LastIndexOf("\\"));
                            if (lastPath.Length == 2)
                            {
                                lastPath += "\\";
                            }
                            firstPath = lastPath;
                        }
                        else if (dentry[actPanel - 1].Name == "....")
                        {
                            upStep = true;
                            if ((dentry[actPanel - 1].Lastpath.Length == 3) && (rootFolder[actPanel - 1]))
                            {
                                getDrives(lv);
                                return;
                            }
                            path = dentry[actPanel - 1].Lastpath;
                            if (dentry[actPanel - 1].Lastpath.Length == 3)
                            {
                                lastPath = path;
                            }
                            else
                            {
                                lastPath = dentry[actPanel - 1].Lastpath.Substring(0, dentry[actPanel - 1].Lastpath.LastIndexOf("\\")); //a szülő mappa
                                if (lastPath.Length == 2)
                                    lastPath += "\\";
                            }
                            firstPath = path;
                        }
                        else
                        {
                            path = dentry[actPanel - 1].Fullpath;
                            lastPath = dentry[actPanel - 1].Fullpath.Substring(0, dentry[actPanel - 1].Fullpath.LastIndexOf("\\")); //szülőfolder (szép határa)
                            if (lastPath.Length == 2)
                            {
                                lastPath += "\\";
                            }
                            firstPath = path;
                        }
                        if (firstPath.Length > 3)
                            rootFolder[actPanel - 1] = false;
                        else
                            rootFolder[actPanel - 1] = true;
                        subEntries.Clear();
                        DirectoryEntry dd = new DirectoryEntry(
                                "....", firstPath, "<Folder>", "<DIR>",
                                dentry[actPanel - 1].Date,
                                PhysicalPath(@"..\..\Images\folder.png"), EntryType.Dir);
                        dd.Lastpath = lastPath;
                        subEntries.Add(dd);

                        if (upStep)
                            dentry[actPanel - 1] = dd;
                        try
                        {
                            foreach (string s in Directory.GetDirectories(path))
                            {
                                DirectoryInfo dir = new DirectoryInfo(s);
                                DirectoryEntry d = new DirectoryEntry(
                                    dir.Name, dir.FullName, "<Folder>", "<DIR>",
                                    Directory.GetLastWriteTime(s),
                                    PhysicalPath(@"..\..\Images\dir.gif"), EntryType.Dir);
                                d.Lastpath = dentry[actPanel - 1].Fullpath;
                                subEntries.Add(d);
                            }
                            foreach (string f in Directory.GetFiles(path))
                            {
                                FileInfo file = new FileInfo(f);
                                DirectoryEntry d = new DirectoryEntry(
                                    file.Name, file.FullName, file.Extension, file.Length.ToString(),
                                    file.LastWriteTime,
                                    PhysicalPath(@"..\..\Images\file.gif"), EntryType.File);
                                subEntries.Add(d);
                            }
                            lv.DataContext = subEntries;
                        }
                        catch { }
                    }
                }
            }
        }

        private void cmdOpen_Click(object sender, RoutedEventArgs e)
        {
            openFile();
        }

        private void cmdCopy_Click(object sender, RoutedEventArgs e)
        {
            execute("COPY");
        }

        private void cmdMove_Click(object sender, RoutedEventArgs e)
        {
            execute("MOVE");
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            execute("DELETE");
        }

        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            if (actPanel == 0)
            {
                MessageBox.Show("Nincs kiválasztva a cél mappa!");
                return;
            }
            if (dentry[actPanel - 1] == null)
            {
                MessageBox.Show("Még nem történt kiválasztás!");
                return;
            }
            string parentFolder = dentry[actPanel - 1].Fullpath;
            string newFolder;
            if (txtfolder.Text != "")
            {
                newFolder = parentFolder + "\\" + txtfolder.Text;
                if (!System.IO.Directory.Exists(newFolder))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(newFolder);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    catch (ArgumentNullException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    txtfolder.Text = "";
                    if (actPanel == 1)
                        panelRefresh(listView1, right_panel_subEntries, parentFolder);
                    else
                        panelRefresh(listView2, left_panel_subEntries, parentFolder);
                }
                else
                {
                    MessageBox.Show(newFolder + " mappa már létezik!");
                    txtfolder.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Nevet kellene adni az új mappának!");
                txtfolder.Text = "";
            }
        }

        private void openFile()
        {
            if (actPanel > 0)
            {
                if (sItem[actPanel - 1] != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(sItem[actPanel - 1]);
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Nincs elem kiválastva.\nElem kiválasztása bal gombbal.");
            }
        }

        private void execute(string cmd)
        {
            if (actPanel == 0)
            {
                System.Windows.MessageBox.Show("Előbb fájlt vagy mappát kellene kiválasztani!");
                return;
            }

            if (dentry[actPanel - 1] == null)
            {
                System.Windows.MessageBox.Show("Előbb fájlt vagy mappát kellene kiválasztani!");
                return;
            }

            if ((cmd == "COPY") || (cmd == "MOVE"))
            {
                if (actPanel == 1)
                {
                    if (dentry[1] == null)
                    {
                        System.Windows.MessageBox.Show("A cél mappát ki kellene választani!");
                        return;
                    }

                }
                else
                {
                    if (dentry[0] == null)
                    {
                        System.Windows.MessageBox.Show("A cél mappát ki kellene választani!");
                        return;
                    }
                    if (dentry[0].IsDrive)
                    {
                        System.Windows.MessageBox.Show("Meghajtókkal nem lehet műveleteket végezni!");
                        return;
                    }
                }
                if (leftSelected.Content.ToString() == "" || rightSelected.Content.ToString() == "")
                {
                    System.Windows.MessageBox.Show("Forrásra és célra van szükség!");
                    return;
                }

                if (leftSelected.Content.ToString() == rightSelected.Content.ToString())
                {
                    System.Windows.MessageBox.Show("Forrásra és célra nem lehet azonos!");
                    return;
                }

            }

            ListView lvCtrl;
            Label lbCtrl;

            bCancel = false;
            overWriteAll = false;
            eztMarKerdeztem = false;
            entryCount = 0;
            setCtrls(false);
            if (actPanel == 1)
            {
                lvCtrl = listView1;
                lbCtrl = leftSelected;
            }
            else
            {
                lvCtrl = listView2;
                lbCtrl = rightSelected;
            }
            if (lvCtrl.SelectedItems.Count == 1)
            {
                if (dentry[actPanel - 1].IsDrive && (sItem[actPanel - 1] == dentry[actPanel - 1].Fullpath))
                {
                    System.Windows.MessageBox.Show("Meghajtóval nem lehet műveleteket végezni!");
                    return;
                }

                if (extension[actPanel - 1] == EntryType.Dir)
                {
                    overWriteAll = true;
                    fileOperation("COUNTER");
                    overWriteAll = false;
                }
                else
                {
                    entryCount = 1;
                }
                fileOperation(cmd);
                if ((useBuiltInCopy || (cmd == "DELETE")) && (entryCount <= 0))
                {
                    setCtrls(true);
                    lvCtrl.UnselectAll();
                    progressBar.Hide();
                    panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                    panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                }
                this.Focus();
            }
            else if (lvCtrl.SelectedItems.Count > 1)
            {
                try
                {
                    tmpentries.Clear();
                    foreach (DirectoryEntry entry in lvCtrl.SelectedItems)
                    {
                        if (bCancel) break;
                        tmpentries.Add(entry);
                        DoEvents();
                    }
                    overWriteAll = true;
                    foreach (DirectoryEntry entry in tmpentries)
                    {
                        sItem[actPanel - 1] = entry.Fullpath;
                        extension[actPanel - 1] = entry.Type;
                        fileOperation("COUNTER");
                        DoEvents();
                    }
                    overWriteAll = false;
                    if (ask(cmd))
                    {
                        foreach (DirectoryEntry entry in tmpentries)
                        {
                            if (bCancel) break;
                            sItem[actPanel - 1] = entry.Fullpath;
                            extension[actPanel - 1] = entry.Type;
                            fileOperation(cmd);
                            DoEvents();
                        }
                        if ((useBuiltInCopy || (cmd == "DELETE")) && (entryCount <= 0))
                        {
                            setCtrls(true);
                            lvCtrl.UnselectAll();
                            progressBar.Hide();
                            panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                            panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                        }
                        this.Focus();
                    }
                    else
                    {
                        lvCtrl.UnselectAll();
                        setCtrls(true);
                    }
                }
                catch
                {
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Nincs kiválastva elem!");
                return;
            }
        }

        private bool ask(string cmd)
        {
            MessageBoxResult result;
            string question = "", subQuestion = "";
            if (!overWriteAll)
            {
                if (cmd == "COPY")
                {
                    question = "Valóban végre akarja hajtani a másolást?";
                    subQuestion = "\n Felülírjak minden létező fájlt és mappát...?";
                }
                else if (cmd == "MOVE")
                {
                    question = "Valóban végre akarja hajtani a mozgatást?";
                    subQuestion = "\n Felülírjak minden létező fájlt és mappát...?";
                }
                else if (cmd == "DELETE")
                {
                    question = "Valóban végre akarja hajtani a törlést?";
                    subQuestion = "\n Töröljek minden létező fájlt és mappát...?";
                }
                result = MessageBox.Show(question, "Ajaj", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (!overWriteAll && !eztMarKerdeztem && entryCount > 1)
                    {
                        MessageBoxResult resultAll = MessageBox.Show(question + subQuestion, "Ajaj", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (resultAll == MessageBoxResult.Yes)
                        {
                            overWriteAll = true;
                        }
                        eztMarKerdeztem = true;
                    }
                    return true;
                }
            }
            return false;
        }

        private void fileOperation(string cmd)
        {
            string sourcePath, targetPath, fName, target;
            EntryType sExt, tExt;
            if (actPanel == 1)
            {
                sourcePath = sItem[0];
                sExt = extension[0];
                targetPath = sItem[1];
                tExt = extension[1];
            }
            else
            {
                sourcePath = sItem[1];
                sExt = extension[1];
                targetPath = sItem[0];
                tExt = extension[0];
            }
            if (((sourcePath != "") && (targetPath != "") && (sourcePath != targetPath)) || ((sourcePath != "") && (cmd == "DELETE")))
            {
                if (((sExt == EntryType.Dir) && (tExt == EntryType.Dir)) || ((sExt == EntryType.Dir) && (cmd == "DELETE")))
                {
                    if (ask(cmd) || overWriteAll)
                    {
                        string sSubFolder = mySubFolder();
                        if (sSubFolder != "")
                            targetPath += "\\" + sSubFolder;
                        workedPath = sourcePath;
                        recursiveExecution(sourcePath, targetPath, cmd);
                    }
                }
                else if (((sExt == EntryType.File) && (tExt == EntryType.Dir)) || ((sExt != EntryType.File) && (cmd == "DELETE")))
                {
                    if (cmd == "COUNTER")
                    {
                        entryCount++;
                    }
                    else
                    {
                        if (cmd == "DELETE")
                            entryCount--;
                        if (cmd == "DELETE")
                        {
                            if (ask(cmd) || overWriteAll)
                            {
                                try
                                {
                                    startProcess(sourcePath, "", cmd);
                                    //                                    System.IO.File.Delete(sourcePath);
                                }
                                catch (UnauthorizedAccessException e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                                catch (ArgumentNullException e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                                catch (System.IO.IOException e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                            }
                        }
                        else
                        {
                            fName = sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
                            target = System.IO.Path.Combine(targetPath, fName);
                            if (System.IO.File.Exists(target))
                            {
                                if (ask(cmd) || overWriteAll)
                                {
                                    try
                                    {
                                        lblfile.Content = entryCount.ToString() + " fájl van még hátra";
                                        if (cmd == "MOVE")
                                            System.IO.File.Delete(target);
                                        startProcess(sourcePath, target, cmd);
                                    }
                                    catch (UnauthorizedAccessException e)
                                    {
                                        MessageBox.Show(e.Message);
                                    }
                                    catch (ArgumentNullException e)
                                    {
                                        MessageBox.Show(e.Message);
                                    }
                                    catch (System.IO.IOException e)
                                    {
                                        MessageBox.Show(e.Message);
                                    }
                                }
                            }
                            else
                            {
                                startProcess(sourcePath, target, cmd);
                            }
                        }
                    }
                }
            }
        }

        void recursiveExecution(string sourceFolder, string destFolder, string cmd)
        {
            if (!bCancel)
            {
                try
                {
                    if (((cmd == "COPY") || (cmd == "MOVE")) && !bCancel)
                    {
                        if (!Directory.Exists(destFolder))
                        {
                            lblfile.Content = destFolder + " mappa létrehozása";
                            this.UpdateLayout();
                            Directory.CreateDirectory(destFolder);
                        }
                    }
                    string name = "", dest = "";
                    string[] files = Directory.GetFiles(sourceFolder);
                    foreach (string file in files)
                    {
                        if (cmd == "COUNTER")
                        {
                            entryCount++;
                        }
                        else
                        {
                            if (cmd == "DELETE")
                            {
                                entryCount--;
                                progressBar.ProgressText = file;
                            }
                            if (bCancel) break;
                            if (cmd == "DELETE")
                            {
                                lblfile.Content = entryCount.ToString() + " fájl van még hátra";
                                //File.Delete(file);
                                startProcess(file, dest, cmd);
                            }
                            else
                            {
                                name = System.IO.Path.GetFileName(file);
                                dest = System.IO.Path.Combine(destFolder, name);
                                if (System.IO.File.Exists(dest))
                                {
                                    if (ask(cmd) || overWriteAll)
                                    {
                                        File.Delete(dest);
                                        startProcess(file, dest, cmd);
                                    }
                                }
                                else
                                {
                                    startProcess(file, dest, cmd);
                                }
                            }
                            DoEvents();
                        }
                    }
                    string[] folders = Directory.GetDirectories(sourceFolder);
                    foreach (string folder in folders)
                    {
                        if (bCancel) break;
                        if ((cmd == "COPY") || (cmd == "MOVE"))
                        {
                            name = System.IO.Path.GetFileName(folder);
                            dest = System.IO.Path.Combine(destFolder, name);
                        }
                        recursiveExecution(folder, dest, cmd);
                        DoEvents();
                    }
                    if (cmd != "COUNTER")
                    {
                        if ((sourceFolder == workedPath) && !useBuiltInCopy && (cmd != "DELETE"))
                        {
                            endProcess(true, cmd);
                        }
                    }
                    if (((cmd == "DELETE") || (cmd == "MOVE")) && !bCancel)
                        Directory.Delete(sourceFolder);
                }
                catch (UnauthorizedAccessException e)
                {
                    MessageBox.Show(e.Message);
                }
                catch (ArgumentNullException e)
                {
                    MessageBox.Show(e.Message);
                }
                catch (System.IO.IOException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        void setCtrls(bool bValue)
        {
            lblfile.Content = "";
            this.UpdateLayout();
            DoEvents();
        }

        string mySubFolder()
        {
            string sSubFolder = "", fName = "";
            if (actPanel > 0)
            {
                fName = sItem[actPanel - 1];
                if (extension[actPanel - 1] == EntryType.Dir)
                {
                    sSubFolder = fName.Substring(fName.LastIndexOf("\\") + 1);
                }
            }
            return sSubFolder;
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public static void DoEvents()
        {
            Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.Background,
            new EmptyDelegate(
            delegate { }));
        }

        private delegate void EmptyDelegate();

        void CancelProcess(object sender, EventArgs e)
        {
            bCancel = true;
        }

        private void startProcess(string source, string target, string cmd)
        {
            lblfile.Content = entryCount.ToString() + " fájl van még hátra";
            progressBar.Show();

            bool startOK = false;
            try
            {
                if (System.IO.File.Exists(target))
                {
                    if (ask(cmd) || overWriteAll)
                        startOK = true;
                }
                else
                    startOK = true;
                if (startOK)
                {
                    if (cmd == "MOVE")
                    {
                        copyx(source, target);
                        File.Delete(source);
                    }
                    else if (cmd == "COPY")
                    {
                        copyx(source, target);
                    }
                    else if (cmd == "DELETE")
                    {
                        File.Delete(source);
                    }
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (System.IO.IOException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void endProcess(bool success, string cmd)
        {
            if (entryCount <= 1)
            {
                try
                {
                    progressBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                            delegate ()
                            {
                                try
                                {
                                    lblfile.Content = "";
                                    if (actPanel == 1)
                                        listView1.UnselectAll();
                                    else
                                        listView2.UnselectAll();
                                    progressBar.Hide();
                                    setCtrls(true);
                                    panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                                    panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                                    DoEvents();
                                }
                                catch (System.NullReferenceException ex)
                                {
                                    System.Windows.MessageBox.Show(ex.Message);
                                }
                            }
                        ));
                }
                catch (System.NullReferenceException)
                {
                    progressBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                            delegate ()
                            {
                                try
                                {
                                    progressBar.Hide();
                                    setCtrls(true);
                                    panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                                    panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                                }
                                catch (System.NullReferenceException ex)
                                {
                                    System.Windows.MessageBox.Show(ex.Message);
                                }
                            }
                        ));
                }
            }
            if (success == true)
            {
                Action del = () =>
                {
                    lblfile.Content = entryCount.ToString() + " fájl van még hátra";
                    entryCount--;
                    if (entryCount > 1)
                        progressBar.Show();
                    else
                        lblfile.Content = "";
                    if (cmd == "COPY")
                    {
                        if (actPanel == 1)
                            panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                        else
                            panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                    }
                    else if (cmd == "DELETE")
                    {
                        if (actPanel == 1)
                            panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                        else
                            panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                    }
                    else if (cmd == "MOVE")
                    {
                        panelRefresh(listView1, left_panel_subEntries, leftSelected.Content.ToString());
                        panelRefresh(listView2, right_panel_subEntries, rightSelected.Content.ToString());
                    }
                    DoEvents();
                };
                Dispatcher.Invoke(del);
            }
        }
        private void copyx(string source, string target)
        {
            progressBar.ProgressText = source;
            progressBar.ProgressValue = 0;
            progressBar.UpdateLayout();
            DoEvents();

            FileInfo file = new FileInfo(source);
            FileInfo destfile = new FileInfo(target);
            if (destfile.Exists && destfile.IsReadOnly)
            {
                MessageBox.Show("A célterületen már létezik a file, és írásvédett. \n A művelet nem hajtódik végre.");
                return;
            }
            byte[] buffer = new byte[4096];
            FileStream fsread = file.Open(FileMode.Open, FileAccess.Read);
            FileStream fswrite = destfile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            long maxbytes = 0;
            long fLength = file.Length;
            double percent;
            int bytesread;
            int maxRefreshCycle = 1;
            int refreshCycle = 0;
            while ((bytesread = fsread.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bCancel) break;
                fswrite.Write(buffer, 0, bytesread);
                maxbytes = maxbytes + bytesread;
                if (refreshCycle >= maxRefreshCycle)
                {
                    if (fLength > 0)
                    {
                        percent = (double)((maxbytes * 100) / fLength);
                        progressBar.ProgressValue = (int)percent;
                        progressBar.UpdateLayout();
                    }
                    DoEvents();
                    refreshCycle = 0;
                }
                refreshCycle++;
            }
            fsread.Flush();
            fswrite.Flush();
            fsread.Close();
            fswrite.Close();
            fsread.Dispose();
            fswrite.Dispose();
            System.IO.File.SetAttributes(target, FileAttributes.Normal);
            progressBar.ProgressText = "";
            progressBar.ProgressValue = 0;
            progressBar.UpdateLayout();
            entryCount--;
            DoEvents();
        }
    }
}
