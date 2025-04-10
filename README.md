# Design Pattern Repository na Clean Architecture
## Guia de Implementação

## Sumário
- [Orientação Geral](#orientação-geral)
- [Clean Architecture - Visão Geral](#clean-architecture---visão-geral)
- [Padrão Repository na Clean Architecture](#padrão-repository-na-clean-architecture)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Implementação](#implementação)
  - [Interface do Repositório (Core)](#interface-do-repositório-core)
  - [Caso de Uso (Application)](#caso-de-uso-application)
  - [Implementações Concretas](#implementações-concretas)
- [Melhores Práticas](#melhores-práticas)
- [FAQ](#faq)

## Orientação Geral

Nossa arquitetura adota a **Clean Architecture** com um foco específico no uso de **consultas SQL diretas**, sem utilização de ORM (como Entity Framework ou Dapper). As razões principais para esta decisão são:

1. Necessidade de lidar com lógica de acesso a dados complexa e altamente customizada
2. Evitar camadas de abstração desnecessárias que dificultam o controle fino sobre as consultas
3. Manter comportamento previsível e performance otimizada diretamente com SQL Server e MySQL

A solução adotada é a implementação do **Design Pattern Repository**, seguindo os princípios da Clean Architecture e utilizando uma **abordagem híbrida** para maximizar tanto a flexibilidade quanto a reutilização de código.

## Clean Architecture - Visão Geral

A Clean Architecture organiza a aplicação em camadas concêntricas, onde cada camada só pode depender da camada mais interna. O fluxo de dependência vai do externo para o núcleo da aplicação:

```
UI → Application → Domain → Infrastructure
```

### Camadas:

1. **Entities (Domain)**: Representam as regras de negócio e são independentes de frameworks
2. **Use Cases (Application)**: Coordenam a lógica de negócio
3. **Interface Adapters** (Controllers, Presenters, Gateways): Adaptam dados entre camadas
4. **Infrastructure** (Banco de dados, frameworks, APIs externas): Implementa detalhes específicos

## Padrão Repository na Clean Architecture

O Repository Pattern fornece uma abstração sobre a persistência de dados, permitindo que a lógica de domínio e de aplicação não dependa de detalhes específicos de banco de dados.

### Objetivos:

- Separar a lógica de negócio do acesso a dados
- Facilitar testes unitários e mocks
- Permitir substituição de tecnologias de persistência sem impacto no domínio
- Fornecer uma abstração clara entre camadas
- Trabalhar diretamente com SQL, mantendo controle e performance

## Estrutura do Projeto

```
📁 MyProject/
├── 📁 Domain/
│   ├── 📁 Entities/
│   │   └── 📄 User.cs
│   └── 📁 Repositories/
│       ├── 📄 IRepository.cs
│       └── 📄 IUserRepository.cs
├── 📁 Application/
│   └── 📁 UseCases/
│       └── 📄 AuthService.cs
└── 📁 Infrastructure/
    └── 📁 Repositories/
        ├── 📄 SqlUserRepository.cs
        └── 📄 MySqlUserRepository.cs
```

## Implementação

### Interface do Repositório (Core)

#### Interface Genérica

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

**Explicação:** Este código define uma interface genérica de repositório que estabelece as operações CRUD básicas para qualquer tipo de entidade. Esta interface segue o princípio de Interface Segregation do SOLID, fornecendo apenas os métodos essenciais para manipulação de dados.

#### Interface Específica

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
```

**Explicação:** Esta interface estende a interface genérica adicionando uma operação específica para a entidade User. Ela demonstra como você pode criar métodos especializados para operações de domínio específicas mantendo a herança da interface genérica.

### Caso de Uso (Application)

```csharp
public class AuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> LoginAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }
}
```

**Explicação:** Este serviço de aplicação (caso de uso) demonstra como consumir o repositório através da interface. É importante notar que ele depende da abstração (IUserRepository) e não da implementação concreta, seguindo o princípio de Inversão de Dependência do SOLID.

### Implementações Concretas

#### SQL Server - Construtor e Inicialização

```csharp
public class SqlUserRepository : IUserRepository
{
    private readonly SqlConnection _connection;

    public SqlUserRepository(SqlConnection connection)
    {
        _connection = connection;
    }
}
```

**Explicação:** O construtor recebe uma conexão SQL como dependência, seguindo o princípio de injeção de dependência. Isto facilita a testabilidade e o gerenciamento do ciclo de vida da conexão fora da classe de repositório.

#### SQL Server - Método GetByIdAsync

```csharp
public async Task<User?> GetByIdAsync(Guid id)
{
    using var cmd = new SqlCommand("SELECT * FROM Users WHERE Id = @Id", _connection);
    cmd.Parameters.AddWithValue("@Id", id);

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new User
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        };
    }
    return null;
}
```

**Explicação:** Este método mostra como implementar uma consulta parametrizada para buscar um usuário por ID. Observe o uso de parâmetros para evitar injeção de SQL e o mapeamento manual dos resultados para a entidade de domínio.

#### SQL Server - Método GetByEmailAsync

```csharp
public async Task<User?> GetByEmailAsync(string email)
{
    using var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", _connection);
    cmd.Parameters.AddWithValue("@Email", email);

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new User
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        };
    }
    return null;
}
```

**Explicação:** Este método implementa a operação específica definida na interface IUserRepository, buscando um usuário pelo email. Note como o código é explícito e direto, sem abstrações desnecessárias.

#### SQL Server - Método GetAllAsync

```csharp
public async Task<IEnumerable<User>> GetAllAsync() 
{
    var users = new List<User>();
    using var cmd = new SqlCommand("SELECT * FROM Users", _connection);
    
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        users.Add(new User
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        });
    }
    
    return users;
}
```

**Explicação:** Demonstra como implementar uma consulta para retornar todos os usuários. Este método percorre cada registro no resultado e mapeia para uma lista de entidades User que é retornada para a aplicação.

#### SQL Server - Método AddAsync

```csharp
public async Task AddAsync(User user) 
{
    using var cmd = new SqlCommand(
        "INSERT INTO Users (Id, Name, Email) VALUES (@Id, @Name, @Email)", 
        _connection);
        
    cmd.Parameters.AddWithValue("@Id", user.Id);
    cmd.Parameters.AddWithValue("@Name", user.Name);
    cmd.Parameters.AddWithValue("@Email", user.Email);
    
    await cmd.ExecuteNonQueryAsync();
}
```

**Explicação:** Este método implementa a operação de inserção de um novo usuário no banco de dados. Observe como os valores são passados como parâmetros para evitar injeção de SQL e como é utilizado ExecuteNonQueryAsync para operações que não retornam dados.

#### SQL Server - Método UpdateAsync

```csharp
public async Task UpdateAsync(User user) 
{
    using var cmd = new SqlCommand(
        "UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id", 
        _connection);
        
    cmd.Parameters.AddWithValue("@Id", user.Id);
    cmd.Parameters.AddWithValue("@Name", user.Name);
    cmd.Parameters.AddWithValue("@Email", user.Email);
    
    await cmd.ExecuteNonQueryAsync();
}
```

**Explicação:** Implementa a atualização de um usuário existente no banco de dados. A consulta SQL especifica quais campos serão atualizados e utiliza o Id como critério para identificar o registro a ser modificado.

#### SQL Server - Método DeleteAsync

```csharp
public async Task DeleteAsync(Guid id) 
{
    using var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @Id", _connection);
    cmd.Parameters.AddWithValue("@Id", id);
    
    await cmd.ExecuteNonQueryAsync();
}
```

**Explicação:** Este método implementa a exclusão de um usuário do banco de dados. Observe como é mantido o padrão de usar parâmetros para a cláusula WHERE, garantindo segurança e performance.

#### MySQL - Construtor e Inicialização

```csharp
public class MySqlUserRepository : IUserRepository
{
    private readonly MySqlConnection _connection;

    public MySqlUserRepository(MySqlConnection connection)
    {
        _connection = connection;
    }
}
```

**Explicação:** Similar ao construtor do SqlUserRepository, mas recebe uma conexão MySql específica. Esta separação de implementações demonstra como a mesma interface pode ter diferentes implementações para diferentes tecnologias de banco de dados.

#### MySQL - Método GetByIdAsync

```csharp
public async Task<User?> GetByIdAsync(Guid id)
{
    using var cmd = new MySqlCommand("SELECT * FROM Users WHERE Id = @Id", _connection);
    cmd.Parameters.AddWithValue("@Id", id);

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new User
        {
            Id = reader.GetGuid("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        };
    }
    return null;
}
```

**Explicação:** Implementação do método para MySQL. Note a diferença principal no acesso aos campos: enquanto SQL Server usa índices baseados em posição, MySQL usa nomes de colunas, o que demonstra as pequenas adaptações necessárias para cada tecnologia.

#### MySQL - Método GetByEmailAsync

```csharp
public async Task<User?> GetByEmailAsync(string email)
{
    using var cmd = new MySqlCommand("SELECT * FROM Users WHERE Email = @Email", _connection);
    cmd.Parameters.AddWithValue("@Email", email);

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new User
        {
            Id = reader.GetGuid("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        };
    }
    return null;
}
```

**Explicação:** Implementação da consulta específica para buscar usuário por email no MySQL. Os demais métodos seguiriam a mesma estrutura do SQL Server, adaptados para a sintaxe e comportamento específicos do MySQL.

## Melhores Práticas

1. **Abordagem Híbrida**: Utilize um repositório genérico para operações CRUD comuns, com interfaces específicas para entidades que possuem operações ou consultas particulares.

2. **Injeção de Dependência**: Injete as interfaces dos repositórios nos serviços de aplicação, nunca as implementações concretas.

3. **Mapeamento Manual**: Faça o mapeamento manual entre entidades de domínio e resultados SQL para manter o controle total sobre a transformação de dados.

4. **Parametrização**: Sempre use parâmetros em consultas SQL para evitar injeção de SQL.

5. **Centralização de SQL**: Considere manter as consultas SQL complexas em arquivos separados ou em constantes para facilitar a manutenção.

6. **Gestão de Conexões**: Implemente um padrão consistente para gerenciar o ciclo de vida das conexões de banco de dados.

7. **Tratamento de Exceções**: Implemente um tratamento de exceções específico para erros de banco de dados.

## FAQ

### Por que não usar ORM?
ORM adiciona uma camada de abstração que, em certos cenários, pode limitar o controle fino sobre as consultas SQL e dificultar a otimização de performance em operações complexas.

### Como garantir a testabilidade?
As interfaces de repositório permitem a criação de mocks facilmente para testes unitários. Para testes de integração, use bancos de dados em memória ou containers Docker.

### Como lidar com transações?
Implemente um Unit of Work pattern para gerenciar transações através de múltiplos repositórios, garantindo a consistência dos dados.

### Como lidar com consultas complexas?
Para consultas complexas, crie métodos específicos nas interfaces de repositório que representam o domínio do problema, mantendo a semântica de negócio nas interfaces.

### Como otimizar performance?
- Use consultas parametrizadas
- Considere procedimentos armazenados para operações críticas
- Implemente cache quando apropriado
- Utilize paginação para grandes conjuntos de dados
