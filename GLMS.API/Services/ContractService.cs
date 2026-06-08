using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.API.Models;

namespace GLMS.API.Services;


public interface IContractService
{
    Task<IEnumerable<Contract>> GetAllAsync(DateTime? fromDate, DateTime? toDate, ContractStatus? status);
    Task<Contract?> GetByIdAsync(int id);
    Task<Contract> CreateAsync(CreateContractDto dto);
    Task<Contract?> UpdateAsync(int id, CreateContractDto dto);
    Task<Contract?> UpdateStatusAsync(int id, ContractStatus newStatus);
    Task<bool> DeleteAsync(int id);
}

public interface IServiceRequestService
{
    Task<IEnumerable<ServiceRequest>> GetAllAsync();
    Task<ServiceRequest?> GetByIdAsync(int id);
    Task<ServiceRequest> CreateAsync(CreateServiceRequestDto dto);
    Task<ServiceRequest?> UpdateAsync(int id, UpdateServiceRequestDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IClientService
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<Client> CreateAsync(Client client);
}

//implementation

public class ContractService : IContractService
{
    private readonly ApiDbContext _context;

    public ContractService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contract>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, ContractStatus? status)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(c => c.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.EndDate <= toDate.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        return await query.OrderByDescending(c => c.Id).ToListAsync();
    }

    public async Task<Contract?> GetByIdAsync(int id)
    {
        return await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contract> CreateAsync(CreateContractDto dto)
    {
        var contract = new Contract
        {
            ClientId = dto.ClientId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            ServiceLevel = dto.ServiceLevel,
            SignedAgreementFilePath = dto.SignedAgreementFilePath  
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        return contract;
    }

    public async Task<Contract?> UpdateAsync(int id, CreateContractDto dto)
    {
        var existing = await _context.Contracts.FindAsync(id);
        if (existing == null) return null;

        existing.ClientId = dto.ClientId;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;
        existing.Status = dto.Status;
        existing.ServiceLevel = dto.ServiceLevel;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Contract?> UpdateStatusAsync(int id, ContractStatus newStatus)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return null;

        contract.Status = newStatus;
        await _context.SaveChangesAsync();
        return contract;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return false;

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return true;
    }
   
}

public class ServiceRequestService : IServiceRequestService
{
    private readonly ApiDbContext _context;

    public ServiceRequestService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
    {
        return await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
    }

    public async Task<ServiceRequest?> GetByIdAsync(int id)
    {
        return await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServiceRequest> CreateAsync(CreateServiceRequestDto dto)
    {
        var contract = await _context.Contracts.FindAsync(dto.ContractId)
            ?? throw new InvalidOperationException("Contract not found.");

        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
            throw new InvalidOperationException(
                "Cannot create a Service Request for an Expired or On Hold contract.");

        var request = new ServiceRequest
        {
            ContractId = dto.ContractId,
            Description = dto.Description,
            USDValue = dto.USDValue,
            ZARCost = dto.ZARCost,
            Status = ServiceRequestStatus.Pending
        };

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ServiceRequest?> UpdateAsync(int id, UpdateServiceRequestDto dto)
    {
        var existing = await _context.ServiceRequests.FindAsync(id);
        if (existing == null) return null;

        existing.Description = dto.Description;
        existing.USDValue = dto.USDValue;
        existing.ZARCost = dto.ZARCost;
        existing.Status = dto.Status;

        await _context.SaveChangesAsync();
        return existing;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null) return false;

        _context.ServiceRequests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ClientService : IClientService
{
    private readonly ApiDbContext _context;

    public ClientService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
        => await _context.Clients.OrderBy(c => c.Name).ToListAsync();

    public async Task<Client?> GetByIdAsync(int id)
        => await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }
}