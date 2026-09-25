using System;
using System.Collections.Generic;
/// <summary>
/// Customer contact details for a repair case.
/// </summary>
class RepairCaseData
{
    public string FirstName;
    public string LastName;
    public string Phone;
}
/// <summary>
/// Repair case for a bike repair, including customer info, findings, parts and status.
/// </summary>
class RepairCase
{
    public string FrameNumber;
    public string Problem;
    public RepairCaseData CustomerInfo;
    public List<string> Findings = new List<string>();
    public List<string> Parts = new List<string>();
    public int Status; // 0 = created, 1 = awaiting approval, 2 = approved, 3 = finished
    public Boolean Approved;
    public decimal TotalPrice;
}

class SparePartCatalog
{
    private Dictionary<string, decimal> prices = new Dictionary<string, decimal>
    {
        { "Gear cable", 150m },
        { "Sprocket", 300m },
        { "Brake pads", 120m }
    };

    /// <summary>
    /// Returns the price of a spare part from the catalog.
    /// </summary>
    /// <param name="partName">The name of the spare part.</param>
    /// <returns>The price in DKK, or 0 if the part is not found.</returns>
    public decimal GetPrice(string partName)
    {
        if (prices.ContainsKey(partName))
        {
            return prices[partName];
        }
        return 0m;
    }
}

class Notifier
{
    /// <summary>
    /// Sends an SMS message to a customer phone number.
    /// </summary>
    /// <param name="phone">The customer's phone number.</param>
    /// <param name="message">The text to send.</param>
    public void SendSms(string phone, String message)
    {
        Console.WriteLine("SMS to " + phone + ": " + message);
    }

    /// <summary>
    /// Leaves a voicemail message for the customer.
    /// </summary>
    /// <param name="phone">The customer's phone number.</param>
    public void LeaveVoicemail(string phone)
    {
        Console.WriteLine($"Voicemail left for {phone}: please call us back regarding your bike.");
    }
}

/**
 * Handles repair cases for Peter Pedal.
 * This service creates cases, registers findings, estimates prices and completes repairs.
 */
class repairService
{
    private List<RepairCase> cases = new List<RepairCase>();
    private SparePartCatalog catalog = new SparePartCatalog();
    private Notifier notifier = new Notifier();

    public const decimal HOURLY_RATE = 450;

    /// <summary>
    /// Creates a new repair case for a customer and stores it in memory.
    /// </summary>
    /// <param name="firstName">Customer's first name.</param>
    /// <param name="lastName">Customer's last name.</param>
    /// <param name="phone">Customer phone number.</param>
    /// <param name="FrameNumber">Bike frame number.</param>
    /// <param name="problem">Description of the bike problem.</param>
    public void CreateCase(string firstName, string lastName, string phone, string FrameNumber, string problem)
    {
        RepairCaseData customer = new RepairCaseData();
        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Phone = phone;

        RepairCase c = new RepairCase();
        c.FrameNumber = FrameNumber;
        c.Problem = problem;
        c.CustomerInfo = customer;
        c.Status = 0;

        cases.Add(c);

        Console.WriteLine($"Case created for {customer.FirstName} {customer.LastName}, frame number {FrameNumber}.");
        Console.WriteLine($"Problem: {problem}");
    }

    /// <summary>
    /// Registers one or more findings for an existing repair case.
    /// </summary>
    /// <param name="frameNumber">The frame number of the bike.</param>
    /// <param name="findings">The list of findings to add to the case.</param>
    public void registerFindings(string frameNumber, List<string> findings)
    {
        RepairCase c = FindCase(frameNumber);
        if (c != null)
        {
            if (findings != null)
            {
                if (findings.Count > 0)
                {
                    foreach (var finding in findings)
                    {
                        if (finding != "")
                        {
                            c.Findings.Add(finding);
                            Console.WriteLine("Finding registered: " + finding);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds spare parts needed for a case based on its registered findings.
    /// </summary>
    /// <param name="frameNumber">The frame number of the bike to inspect.</param>
    public void LookUpParts(string frameNumber)
    {
        RepairCase c = FindCase(frameNumber);

        foreach (var finding in c.Findings)
        {
            string stlnr = c.FrameNumber;
            if (finding.Contains("Gear cable"))
            {
                c.Parts.Add("Gear cable");
                Console.WriteLine($"Found part for case {stlnr}: Gear cable ({catalog.GetPrice("Gear cable")} kr)");
            }
            else if (finding.Contains("Sprocket"))
            {
                c.Parts.Add("Sprocket");
                Console.WriteLine($"Found part for case {stlnr}: Sprocket ({catalog.GetPrice("Sprocket")} kr)");
            }
            else if (finding.Contains("Brake pads"))
            {
                c.Parts.Add("Brake pads");
                Console.WriteLine($"Found part for case {stlnr}: Brake pads ({catalog.GetPrice("Brake pads")} kr)");
            }
        }

        Int32 numberOfParts = c.Parts.Count;
        Console.WriteLine($"Found {numberOfParts} part(s) for case {frameNumber}.");
    }

    /// <summary>
    /// Calculates the final price for a gear cable including markup.
    /// </summary>
    /// <returns>The calculated price for the gear cable.</returns>
    public decimal CalculatePriceForGearCable()
    {
        decimal price = 150m;
        decimal markup = price * 0.1m;
        return price + markup;
    }

    /// <summary>
    /// Calculates the final price for a sprocket including markup.
    /// </summary>
    /// <returns>The calculated price for the sprocket.</returns>
    public decimal CalculatePriceForSprocket()
    {
        decimal price = 300m;
        decimal markup = price * 0.1m;
        return price + markup;
    }

    /// <summary>
    /// Calculates the final price for brake pads including markup.
    /// </summary>
    /// <returns>The calculated price for the brake pads.</returns>
    public decimal CalculatePriceForBrakePad()
    {
        decimal price = 120m;
        decimal markup = price * 0.1m;
        return price + markup;
    }

    /// <summary>
    /// Calculates the total price estimate for a repair offer.
    /// </summary>
    /// <param name="c">The repair case used for the estimate.</param>
    /// <returns>The estimated total price including VAT.</returns>
    public decimal BeregnPris(RepairCase c)
    {
        decimal partsPrice = CalculatePriceForGearCable() + CalculatePriceForSprocket() + CalculatePriceForBrakePad();
        decimal labor = HOURLY_RATE * 2;
        decimal subtotal = partsPrice + labor;
        decimal vat = subtotal * 0.25m;
        return subtotal + vat;
    }

    /// <summary>
    /// Creates a price offer for a repair case and leaves a voicemail to the customer.
    /// </summary>
    /// <param name="frameNumber">The frame number of the bike.</param>
    public void CalculateOffer(string frameNumber)
    {
        RepairCase c = FindCase(frameNumber);
        decimal Price = BeregnPris(c);
        c.TotalPrice = Price;
        c.Status = 1;

        int d = 3;
        string cstTlf = c.CustomerInfo.Phone;
        Console.WriteLine($"Offer for case {frameNumber}: {Price.ToString("F2")} kr, delivery in {d} days.");
        Console.WriteLine($"Calling {cstTlf}...");
        notifier.LeaveVoicemail(cstTlf);
    }

    /// <summary>
    /// Approves the repair offer for a specific case.
    /// </summary>
    /// <param name="frameNumber">The frame number of the bike.</param>
    public void ApproveCase(string frameNumber)
    {
        RepairCase c = FindCase(frameNumber);
        c.Approved = true;
        c.Status = 2;
        Console.WriteLine($"{c.CustomerInfo.FirstName} accepted the offer.");
    }

    /// <summary>
    /// Simulates the bike being repaired by Sofia.
    /// </summary>
    /// <param name="frameNumber">The frame number of the bike being repaired.</param>
    public void PimpMyBike(string frameNumber) {
        RepairCase c = FindCase(frameNumber);
        Console.WriteLine($"Sofia is repairing the bike, frame number {c.FrameNumber}...");
    }

    /// <summary>
    /// Calculates the final total price for the repair receipt.
    /// </summary>
    /// <param name="c">The repair case to calculate the total for.</param>
    /// <returns>The final total including VAT.</returns>
    public decimal CalculateTotal(RepairCase c)
    {
        decimal partsPrice = CalculatePriceForGearCable() + CalculatePriceForSprocket() + CalculatePriceForBrakePad();
        decimal labor = HOURLY_RATE * 2;
        decimal subtotal = partsPrice + labor;
        decimal vat = subtotal * 0.25m;
        return subtotal + vat;
    }

    /// <summary>
    /// Completes the repair, sends a receipt message, and marks the case as finished.
    /// </summary>
    /// <param name="frameNumber">The frame number of the completed repair.</param>
    public void finishRepair(string frameNumber)
    {
        RepairCase c = FindCase(frameNumber);

        if (c.Status == 2 && c.Parts.Count > 0 && c.Approved)
        {
            decimal total = CalculateTotal(c);

            if (total < 0)
            {
                Console.WriteLine("Error: negative price");
            }

            c.TotalPrice = total;
            c.Status = 3;

            String message = "Hi " + c.CustomerInfo.FirstName + ", your bike is ready for pickup!";
            notifier.SendSms(c.CustomerInfo.Phone, message);

            Console.WriteLine("--- Receipt ---");
            Console.WriteLine("Frame number: " + c.FrameNumber);
            Console.WriteLine("Total: " + Math.Round(total, 2) + " kr");
        }
    }

    /// <summary>
    /// Confirms that a customer has paid for the repair and the bike is ready.
    /// </summary>
    /// <param name="frameNumber">The frame number of the paid case.</param>
    public void PayCase(string frameNumber)
    {
        var result = FindCase(frameNumber);
        Console.WriteLine($"{result.CustomerInfo.FirstName} has paid {result.TotalPrice.ToString("F2")} kr. The bike is ready to ride!");
    }

    /// <summary>
    /// Prints a summary of the repair case.
    /// </summary>
    /// <param name="frameNumber">The frame number of the case to summarize.</param>
    public void PrintCaseSummary(string frameNumber) {
	RepairCase c = FindCase(frameNumber);
	Console.WriteLine("Case summary for " + c.FrameNumber + ": " + c.Problem);
    }

    private RepairCase FindCase(string frameNumber)
    {
        foreach (var c in cases)
        {
            if (c.FrameNumber == frameNumber)
            {
                return c;
            }
        }
        return null;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var service = new repairService();

        service.CreateCase("Egon", "Cykelmyggen", "20123456", "STL-4471", "The gears are not shifting properly and the bike is almost impossible to ride.");
        service.registerFindings("STL-4471", new List<string> { "Gear cable needs replacement", "Sprocket is worn", "Brake pads are worn" });
        service.LookUpParts("STL-4471");
        service.CalculateOffer("STL-4471");
        service.ApproveCase("STL-4471");
        service.PimpMyBike("STL-4471");
        service.finishRepair("STL-4471");
        service.PayCase("STL-4471");
    }
}