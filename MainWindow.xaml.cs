using DayShift_Overview_Kaufland.Models;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DayShift_Overview_Kaufland;

public partial class MainWindow : Window
{
    public List<User>? users;
    public ObservableCollection<Shift> Shifts { get; set; } = new ObservableCollection<Shift>();
    public ObservableCollection<Date> Dates { get; set; } = new ObservableCollection<Date>();

    public MainWindow()
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
        DataContext = this;
        FetchShiftsByDate(DateTime.UtcNow.Date);
    }

    private void Date_Change(object sender, SelectionChangedEventArgs e)
    {
        DateTime? date = DatePicker.SelectedDate;
        if (date.HasValue)
        {
            DateTime selectedDate = date.Value;
            FetchShiftsByDate(selectedDate);
            KontrolaStavuDna();
        }
    }

    private void FetchShiftsByDate(DateTime selectedDate)
    {
        var shiftCollection = App.Database.GetCollection<Shift>("shifts");

        var startOfDayUTC = selectedDate.Date.ToUniversalTime();
        var endOfDayUTC = startOfDayUTC.AddDays(1).AddSeconds(-1);

        var filter = Builders<Shift>.Filter.Gte(s => s.ShiftDate, startOfDayUTC) &
                     Builders<Shift>.Filter.Lte(s => s.ShiftDate, endOfDayUTC);

        var shifts = shiftCollection.Find(filter).ToList();

        Shifts.Clear();
        foreach (var shift in shifts)
        {
            Shifts.Add(shift);
        }

        MessageBox.Show($"Found {shifts.Count} shifts for {selectedDate.ToShortDateString()}");
    }

    private void KontrolaStavuDna()
    {
        DateTime? date = DatePicker.SelectedDate;
        if (date.HasValue)
        {
            var dateCollection = App.Database.GetCollection<Date>("dates");
            var startOfDayUTC = date.Value.Date.ToUniversalTime();
            var endOfDayUTC = startOfDayUTC.AddDays(1).AddSeconds(-1);

            var filter = Builders<Date>.Filter.Gte(s => s.ShiftDate, startOfDayUTC) &
                         Builders<Date>.Filter.Lte(s => s.ShiftDate, endOfDayUTC);

            var foundDate = dateCollection.Find(filter).FirstOrDefault();

            if (foundDate != null && foundDate.IsClosed == true)
            {
                MessageBox.Show("Obchodny den je uzatvoreny.");
                SetAllTimeInputsReadOnly(ShiftsDataGrid, true);
                SetAllButtonsEnabled(this, false);
                
            }
            else
            {
                SetAllTimeInputsReadOnly(ShiftsDataGrid, false);
                SetAllButtonsEnabled(this, true);
               
            }

        }
    }

    private void SetAllTimeInputsReadOnly(DependencyObject parent, bool isReadOnly)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBox textBox && (textBox.Name as string == "CasPrichodu" || textBox.Name as string == "CasOdchodu"))
            {
                textBox.IsReadOnly = isReadOnly;
            }

            SetAllTimeInputsReadOnly(child, isReadOnly);
            
            

        }
    }
    private void SetAllButtonsEnabled(DependencyObject parent, bool isEnabled)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is DatePicker)
                continue;

            if (child is Button button)
            {
                button.IsEnabled = isEnabled;
            }


            SetAllButtonsEnabled(child, isEnabled);
        }
    }


    private void PotvrdenieNastupu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Shift shift)
        {
            var shiftCollection = App.Database.GetCollection<Shift>("shifts");

         
            bool newState = !shift.ArrivalConfirmed;

            var filter = Builders<Shift>.Filter.Eq(s => s.Id, shift.Id);
            var update = Builders<Shift>.Update.Set(s => s.ArrivalConfirmed, newState);
            shiftCollection.UpdateOne(filter, update);

            shift.ArrivalConfirmed = newState;

            button.Content = shift.ArrivalConfirmed ? "V" : "potvrdit";
        }
    }

    private void PotvrdenieUkoncenia_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Shift shift)
        {
            var shiftCollection = App.Database.GetCollection<Shift>("shifts");

            bool newState = !shift.DepartureConfirmed;

            var filter = Builders<Shift>.Filter.Eq(s => s.Id, shift.Id);
            var update = Builders<Shift>.Update.Set(s => s.DepartureConfirmed, newState);
            shiftCollection.UpdateOne(filter, update);

            shift.DepartureConfirmed = newState;

            button.Content = shift.DepartureConfirmed ? "V" : "potvrdit";
        }
    }

    private void UzatvoritObchodnyDen_Click(object sender, RoutedEventArgs e)
    {
        DateTime? date = DatePicker.SelectedDate;
        if (date.HasValue)
        {
            var shiftCollection = App.Database.GetCollection<Shift>("shifts");
            var startOfDayUTC = date.Value.Date.ToUniversalTime();
            var endOfDayUTC = startOfDayUTC.AddDays(1).AddSeconds(-1);

            var shiftFilter = Builders<Shift>.Filter.Gte(s => s.ShiftStart, startOfDayUTC) &
                              Builders<Shift>.Filter.Lte(s => s.ShiftStart, endOfDayUTC);

            var shiftsOnThatDay = shiftCollection.Find(shiftFilter).ToList();

            bool allConfirmed = shiftsOnThatDay.All(s => s.ArrivalConfirmed == true) && shiftsOnThatDay.All(s => s.DepartureConfirmed == true);

            if (!allConfirmed)
            {
                MessageBox.Show("Nie všetky príchody boli potvrdené.");
                return;
            }

            var dateCollection = App.Database.GetCollection<Date>("dates");

            var dateFilter = Builders<Date>.Filter.Gte(s => s.ShiftDate, startOfDayUTC) &
                             Builders<Date>.Filter.Lte(s => s.ShiftDate, endOfDayUTC);

            var updateDate = Builders<Date>.Update.Set(s => s.IsClosed, true);
            dateCollection.UpdateOne(dateFilter, updateDate);

            MessageBox.Show("Obchodný deň bol úspešne uzatvorený.");
        }
    }

    private void ZmenaCasuPrichodu(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var shift = textBox.DataContext as Shift;
            if (shift == null) return;

            if (DatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = DatePicker.SelectedDate.Value;
                var upravenyPrichodString = textBox.Text;

                if (DateTime.TryParse(upravenyPrichodString, out DateTime timeOnly))
                {
                    var upravenyPrichod = new DateTime(
                        selectedDate.Year,
                        selectedDate.Month,
                        selectedDate.Day,
                        timeOnly.Hour,
                        timeOnly.Minute,
                        timeOnly.Second
                    );

                    OdoslanieCasuPrichodu(shift, upravenyPrichod);
                }
                else
                {
                    MessageBox.Show("Neplatný formát času.");
                }
            }
        }
    }

    private void ZmenaCasuOdchodu(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var shift = textBox.DataContext as Shift;
            if (shift == null) return;
            if (DatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = DatePicker.SelectedDate.Value;
                var upravenyOdchodString = textBox.Text;
                if (DateTime.TryParse(upravenyOdchodString, out DateTime timeOnly))
                {
                    var upravenyOdchod = new DateTime(
                        selectedDate.Year,
                        selectedDate.Month,
                        selectedDate.Day,
                        timeOnly.Hour,
                        timeOnly.Minute,
                        timeOnly.Second
                    );
                    OdoslanieCasuOdchodu(shift, upravenyOdchod);
                }
                else
                {
                    MessageBox.Show("Neplatný formát času.");
                }
            }
        }
    }

    private void OdoslanieCasuPrichodu(Shift shift, DateTime novyPrichod)
    {
        var shiftCollection = App.Database.GetCollection<Shift>("shifts");
        var filter = Builders<Shift>.Filter.Eq(s => s.Id, shift.Id);
        var update = Builders<Shift>.Update.Set(s => s.ShiftStart, novyPrichod);

        shiftCollection.UpdateOne(filter, update);
    }

    private void OdoslanieCasuOdchodu(Shift shift, DateTime novyOdchod)
    {
        var shiftCollection = App.Database.GetCollection<Shift>("shifts");
        var filter = Builders<Shift>.Filter.Eq(s => s.Id, shift.Id);
        var update = Builders<Shift>.Update.Set(s => s.ShiftEnd, novyOdchod);

        shiftCollection.UpdateOne(filter, update);
    }
}