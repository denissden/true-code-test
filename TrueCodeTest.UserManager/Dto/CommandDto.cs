using System.Text.Json.Serialization;

namespace TrueCodeTest.UserManager.Dto;

/// The base class for representing a command data transfer object (DTO).
/// This class is used as a marker for identifying command types during serialization and deserialization.
/// It also enables polymorphic behavior by allowing derived classes to be serialized and deserialized.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "CommandType")]
[JsonDerivedType(typeof(GetByIdAndDomainCommand), "get_user")]
[JsonDerivedType(typeof(ListByDomainCommand), "list_domain_users")]
[JsonDerivedType(typeof(FindByTag), "find_users_by_tag")]
public record CommandDto();

public record GetByIdAndDomainCommand(Guid Id, string Domain) : CommandDto;
public record ListByDomainCommand(string Domain, int PageNumber, int PageSize) : CommandDto;
public record FindByTag(string Domain, string Tag) : CommandDto;
