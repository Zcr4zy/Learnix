# 📚 Learnix

Sistema de gerenciamento de cursos, feito com C#. O **Learnix** é uma API REST construída em ASP.NET Core com o objetivo de permitir que usuários se inscrevam em cursos, enquanto administradores cuidam da criação e manutenção desses cursos.  

Futuramente, a aplicação será integrada com um front-end em Angular ou Vue.

---

## 🚀 Tecnologias Utilizadas

- ✅ ASP.NET Core 9 (Web API)
- ✅ C# moderno
- ✅ Entity Framework Core
- ✅ SQL Server
- ✅ JWT (Json Web Token) para autenticação
- ✅ DTOs para separação entre Models e transporte de dados

---

## 🔐 Autenticação & Roles

O sistema conta com autenticação via **JWT**, e cada usuário é atribuído a uma role:

- `Admin`: pode cadastrar, editar e excluir cursos
- `Usuario`: pode se inscrever ou sair de cursos existentes

Cada role é definida usando um `enum`, e protegida com `[Authorize(Roles = "...")]`.

---

## 🧠 Funcionalidades

| Módulo         | Funcionalidades                                                                 |
|----------------|----------------------------------------------------------------------------------|
| 👤 Usuários    | Cadastro, login, visualização e alteração de dados pessoais                     |
| 🔐 Auth        | Geração de token JWT, verificação de role                                        |
| 📚 Cursos      | Admin pode criar, editar, excluir e listar cursos                                |
| 🎓 Inscrição   | Usuário pode se inscrever em cursos, ver cursos inscritos ou cancelar inscrição |


