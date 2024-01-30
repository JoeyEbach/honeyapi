using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer> 
{
    new Customer()
    {
        Id = 1,
        Name = "Debra Deter",
        Address = "2343 Bankoff Lane"
    },
    new Customer()
    {
        Id = 2,
        Name = "John Alex",
        Address = "5647 Hillside Road"
    },
    new Customer()
    {
        Id = 3,
        Name = "Bob Rune",
        Address = "9878 Fenley Drive"
    }
};
List<Employee> employees = new List<Employee> 
{
    new Employee()
    {
        Id = 1,
        Name = "Ed Warden",
        Specialty = "Being awesome"
    },
    new Employee()
    {
        Id = 2,
        Name = "Donna Paulson",
        Specialty = "Kicking ass"
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket> 
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "I'm having trouble with my purchase!",
        Emergency = false,
        DateCompleted = new DateTime(2024, 01, 12)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Someone stole all my money!",
        Emergency = true,
        DateCompleted = new DateTime(2023, 02, 12)
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "I just need someone to talk to.",
        Emergency = false,
        DateCompleted = new DateTime(2021, 04, 12)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "I'm prepared to sue!",
        Emergency = true,
        DateCompleted = null
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 1,
        EmployeeId = null,
        Description = "I am requesting a refund!",
        Emergency = false,
        DateCompleted = null
    },
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/servicetickets", () => 
{
    return serviceTickets;
});

app.MapGet("/api/serviceTickets/{id}", (int id) => 
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(s => s.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Customer = customers.FirstOrDefault(customers => customers.Id == serviceTicket.CustomerId);

    serviceTicket.Employee = employees.FirstOrDefault(employees => employees.Id == serviceTicket.EmployeeId);

    return Results.Ok(serviceTicket);
});

app.MapPut("/api/serviceTickets/{id}", (int id, ServiceTicket serviceTicket) => 
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapDelete("/api/serviceTickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.NoContent();
});

app.MapPost("/api/servicetickets/{id}/complete", (int id) => 
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/employees", () => 
{
    return employees;
});

app.MapGet("/api/employees/{id}", (int id) => 
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/api/customers", () =>
{
    return customers;
});

app.MapGet("/api/customers/{id}", (int id) => 
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/api/serviceTickets", (ServiceTicket serviceTicket) => 
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

// 1. Emergencies
// Create an endpoint to return all of the service tickets that are incomplete and are emergencies
app.MapGet("/api/serviceTickets/emergencies", () => 
{
    List<ServiceTicket> emergencyTickets = serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == new DateTime()).ToList();
    if (emergencyTickets == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(emergencyTickets);
});

// 2. Unassigned
// Create an endpoint to return all currently unassigned service tickets
app.MapGet("/api/serviceTickets/unassigned", () => 
{
    List<ServiceTicket> unassignedTicket = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    if (unassignedTicket == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(unassignedTicket);
});

// 3. Inactive Customers
// Create an endpoint to return all of the customers that haven't had a service ticket closed for them in over a year (refer to the explorer chapter in Book 1 on calculating DateTimes).
app.MapGet("/api/customers/inactive", () => 
{
    DateTime yearAgo = DateTime.Now.AddYears(-1);
    List<ServiceTicket> oldTickets = serviceTickets.Where(st => st.DateCompleted < yearAgo && st.DateCompleted != new DateTime()).ToList();
     List<Customer> customerResult = new List<Customer>();
    foreach(ServiceTicket ticket in oldTickets)
    {
        Customer person = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        customerResult.Add(person);
    }
   
    if (customerResult == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(customerResult);
});

// 4. Available employees
// Create an endpoint to return employees not currently assigned to an incomplete service ticket
app.MapGet("/api/serviceTickets/availableemployees", () => 
{
    var assignedEmployees = serviceTickets.Where(st => st.EmployeeId.HasValue && st.DateCompleted == null).Select(st => st.EmployeeId.Value).ToList();
    var availableEmployees = employees.Where(e => !assignedEmployees.Contains(e.Id)).ToList();
    return Results.Ok(availableEmployees);
});

// 5. Employee's customers
// Create an endpoint to return all of the customers for whom a given employee has been assigned to a service ticket (whether completed or not)

app.MapGet("/api/customer/byemployee/{id}", (int id) => 
{
    var tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList(); 
    List<Customer> result = new(); 
    foreach (ServiceTicket ticket in tickets)
    {
        Customer customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        result.Add(customer);
    }

    return Results.Ok(result.Distinct().ToList());
});

// 6. Employee of the month
// Create and endpoint to return the employee who has completed the most service tickets last month.
app.MapGet("/api/employee/ofthemonth", () => 
{
    var tickets = serviceTickets.Where(st => st.DateCompleted > DateTime.Now.AddMonths(-1) && st.DateCompleted <= DateTime.Now).Select(c => c.EmployeeId.Value).ToList();
    var result = tickets.GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).SelectMany(x => x).ToList();

    var answer = employees.Where(e => result.First() == e.Id);

    return Results.Ok(answer);
});

// 7. Past Ticket review
// Create an endpoint to return completed tickets in order of the completion data, oldest first. (This will required a Linq method you haven't learned yet...)

app.MapGet("/api/serviceTickets/insequence", () => 
{
    var tickets = serviceTickets.Where(s => s.DateCompleted != null).OrderBy(st => st.DateCompleted.Value).ToList();
    return Results.Ok(tickets);
});

// 8. Prioritized Tickets (challenge)
// Create an endpoint to return all tickets that are incomplete, in order first by whether they are emergencies, then by whether they are assigned or not (unassigned first).

app.MapGet("/api/servicetickets/priority", () => 
{
    var tickets = serviceTickets.Where(st => st.DateCompleted == null).OrderBy(x => x.Emergency ? 0 : 1).ThenBy(s => s.EmployeeId == null ? 0 : 1).ToList();

    return Results.Ok(tickets);
});



app.Run();
