USE master;
GO

-- Verifica si la base de datos existe y elimínala
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ByeGolden')
    DROP DATABASE ByeGolden;
GO

-- Crea la nueva base de datos
CREATE DATABASE ByeGolden;

GO

USE ByeGolden

GO

CREATE TABLE [EstadosReserva] (
    [IdEstadoReserva] int NOT NULL IDENTITY,
    [NombreEstadoReserva] varchar(15) NULL,
    CONSTRAINT [PK__EstadosR__3E654CA8F00A18AA] PRIMARY KEY ([IdEstadoReserva])
);
GO

CREATE TABLE [Imagenes] (
    [IdImagen] int NOT NULL IDENTITY,
    [UrlImagen] nvarchar(max) NULL,
    CONSTRAINT [PK__Imagenes__B42D8F2AAB297AF8] PRIMARY KEY ([IdImagen])
);
GO

CREATE TABLE [MetodoPago] (
    [IdMetodoPago] int NOT NULL IDENTITY,
    [NomMetodoPago] varchar(20) NULL,
    CONSTRAINT [PK__MetodoPa__6F49A9BE58392942] PRIMARY KEY ([IdMetodoPago])
);
GO

CREATE TABLE [Permisos] (
    [IdPermiso] int NOT NULL IDENTITY,
    [NomPermiso] varchar(20) NULL,
    CONSTRAINT [PK__Permisos__0D626EC8CD4E913C] PRIMARY KEY ([IdPermiso])
);
GO

CREATE TABLE [Roles] (
    [IdRol] int NOT NULL IDENTITY,
    [NomRol] varchar(20) NOT NULL,
    [Estado] bit NULL,
    CONSTRAINT [PK__Roles__2A49584C00E45C16] PRIMARY KEY ([IdRol])
);
GO

CREATE TABLE [TipoDocumento] (
    [IdTipoDocumento] int NOT NULL IDENTITY,
    [NomTipoDcumento] varchar(20) NULL,
    CONSTRAINT [PK__TipoDocu__3AB3332F1789C88E] PRIMARY KEY ([IdTipoDocumento])
);
GO

CREATE TABLE [TipoHabitaciones] (
    [IdTipoHabitacion] int NOT NULL IDENTITY,
    [NomTipoHabitacion] varchar(20) NOT NULL,
    [NumeroPersonas] int NOT NULL,
    [Estado] bit NULL,
    CONSTRAINT [PK__TipoHabi__AB75E87C85C35D57] PRIMARY KEY ([IdTipoHabitacion])
);
GO

CREATE TABLE [TipoServicios] (
    [IdTipoServicio] int NOT NULL IDENTITY,
    [NombreTipoServicio] varchar(50) NOT NULL,
    CONSTRAINT [PK__TipoServ__E29B3EA7589E9CD2] PRIMARY KEY ([IdTipoServicio])
);
GO

CREATE TABLE [PermisosRoles] (
    [IdPermisosRoles] int NOT NULL IDENTITY,
    [IdRol] int NULL,
    [IdPermiso] int NULL,
    CONSTRAINT [PK__Permisos__8C257ED9ED17F213] PRIMARY KEY ([IdPermisosRoles]),
    CONSTRAINT [FK__PermisosR__IdPer__3C69FB99] FOREIGN KEY ([IdPermiso]) REFERENCES [Permisos] ([IdPermiso]),
    CONSTRAINT [FK__PermisosR__IdRol__3B75D760] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([IdRol])
);
GO

CREATE TABLE [Usuarios] (
    [NroDocumento] int NOT NULL,
    [IdTipoDocumento] int NULL,
    [Nombres] varchar(20) NULL,
    [Apellidos] varchar(20) NULL,
    [Celular] varchar(10) NULL,
    [Correo] varchar(100) NULL,
    [Contrasena] varchar(200) NULL,
    [Restablecer] bit NOT NULL,
    [Confirmado] bit NOT NULL,
    [Token] varchar(200) NULL,
    [Estado] bit NULL,
    [IdRol] int NULL,
    CONSTRAINT [PK__Usuarios__CC62C91C9B22FA42] PRIMARY KEY ([NroDocumento]),
    CONSTRAINT [FK__Usuarios__IdRol__412EB0B6] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([IdRol])
);
GO

CREATE TABLE [Clientes] (
    [NroDocumento] varchar(50) NOT NULL,
    [IdTipoDocumento] int NULL,
    [Nombres] varchar(20) NULL,
    [Apellidos] varchar(20) NULL,
    [Celular] varchar(10) NULL,
    [Correo] varchar(100) NULL,
    [Contrasena] varchar(200) NULL,
    [Restablecer] bit NOT NULL,
    [Confirmado] bit NOT NULL,
    [Token] varchar(200) NULL,
    [Estado] bit NULL,
    [IdRol] int NULL,
    CONSTRAINT [PK__Clientes__CC62C91CC0A912D3] PRIMARY KEY ([NroDocumento]),
    CONSTRAINT [FK__Clientes__IdRol__619B8048] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([IdRol]),
    CONSTRAINT [FK__Clientes__IdTipo__628FA481] FOREIGN KEY ([IdTipoDocumento]) REFERENCES [TipoDocumento] ([IdTipoDocumento])
);
GO

CREATE TABLE [Habitaciones] (
    [IdHabitacion] int NOT NULL IDENTITY,
    [IdTipoHabitacion] int NOT NULL,
    [Nombre] varchar(20) NOT NULL,
    [Estado] bit NULL,
    [Descripcion] varchar(50) NOT NULL,
    [Costo] float NOT NULL,
    CONSTRAINT [PK__Habitaci__8BBBF901976A3AD4] PRIMARY KEY ([IdHabitacion]),
    CONSTRAINT [FK__Habitacio__IdTip__45F365D3] FOREIGN KEY ([IdTipoHabitacion]) REFERENCES [TipoHabitaciones] ([IdTipoHabitacion]) ON DELETE CASCADE
);
GO

CREATE TABLE [Servicios] (
    [IdServicio] int NOT NULL IDENTITY,
    [IdTipoServicio] int NOT NULL,
    [NomServicio] varchar(20) NOT NULL,
    [Costo] float NOT NULL,
    [Descripcion] varchar(50) NOT NULL,
    [Estado] bit NULL,
    CONSTRAINT [PK__Servicio__2DCCF9A2D5E127F6] PRIMARY KEY ([IdServicio]),
    CONSTRAINT [FK__Servicios__IdTip__4E88ABD4] FOREIGN KEY ([IdTipoServicio]) REFERENCES [TipoServicios] ([IdTipoServicio]) ON DELETE CASCADE
);
GO

CREATE TABLE [Reservas] (
    [IdReserva] int NOT NULL IDENTITY,
    [NroDocumentoCliente] varchar(50) NOT NULL,
    [NroDocumentoUsuario] int NULL,
    [FechaReserva] date NOT NULL,
    [FechaInicio] date NOT NULL,
    [FechaFinalizacion] date NOT NULL,
    [SubTotal] float NULL,
    [Descuento] float NULL,
    [IVA] float NULL,
    [MontoTotal] float NOT NULL,
    [MetodoPago] int NOT NULL,
    [NroPersonas] int NULL,
    [IdEstadoReserva] int NULL,
    CONSTRAINT [PK__Reservas__0E49C69D28FB0470] PRIMARY KEY ([IdReserva]),
    CONSTRAINT [FK__Reservas__IdEsta__6B24EA82] FOREIGN KEY ([IdEstadoReserva]) REFERENCES [EstadosReserva] ([IdEstadoReserva]),
    CONSTRAINT [FK__Reservas__Metodo__6C190EBB] FOREIGN KEY ([MetodoPago]) REFERENCES [MetodoPago] ([IdMetodoPago]) ON DELETE CASCADE,
    CONSTRAINT [FK__Reservas__NroDoc__693CA210] FOREIGN KEY ([NroDocumentoCliente]) REFERENCES [Clientes] ([NroDocumento]) ON DELETE CASCADE,
    CONSTRAINT [FK__Reservas__NroDoc__6A30C649] FOREIGN KEY ([NroDocumentoUsuario]) REFERENCES [Usuarios] ([NroDocumento])
);
GO

CREATE TABLE [ImagenHabitacion] (
    [IdImagenHabi] int NOT NULL IDENTITY,
    [IdImagen] int NULL,
    [IdHabitacion] int NULL,
    CONSTRAINT [PK__ImagenHa__5B5FF6AD15F092AA] PRIMARY KEY ([IdImagenHabi]),
    CONSTRAINT [FK__ImagenHab__IdHab__49C3F6B7] FOREIGN KEY ([IdHabitacion]) REFERENCES [Habitaciones] ([IdHabitacion]),
    CONSTRAINT [FK__ImagenHab__IdIma__48CFD27E] FOREIGN KEY ([IdImagen]) REFERENCES [Imagenes] ([IdImagen])
);
GO

CREATE TABLE [Paquetes] (
    [IdPaquete] int NOT NULL IDENTITY,
    [NomPaquete] varchar(20) NOT NULL,
    [Costo] float NOT NULL,
    [IdHabitacion] int NOT NULL,
    [Estado] bit NOT NULL,
    [Descripcion] varchar(50) NOT NULL,
    CONSTRAINT [PK__Paquetes__DE278F8B6B12E047] PRIMARY KEY ([IdPaquete]),
    CONSTRAINT [FK__Paquetes__IdHabi__5535A963] FOREIGN KEY ([IdHabitacion]) REFERENCES [Habitaciones] ([IdHabitacion]) ON DELETE CASCADE
);
GO

CREATE TABLE [ImagenServicio] (
    [IdImagenServi] int NOT NULL IDENTITY,
    [IdImagen] int NULL,
    [IdServicio] int NULL,
    CONSTRAINT [PK__ImagenSe__3C03784C62B72875] PRIMARY KEY ([IdImagenServi]),
    CONSTRAINT [FK__ImagenSer__IdIma__5165187F] FOREIGN KEY ([IdImagen]) REFERENCES [Imagenes] ([IdImagen]),
    CONSTRAINT [FK__ImagenSer__IdSer__52593CB8] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO

CREATE TABLE [Abono] (
    [IdAbono] int NOT NULL IDENTITY,
    [IdReserva] int NULL,
    [FechaAbono] date NULL,
    [ValorDeuda] float NULL,
    [Porcentaje] float NULL,
    [Pendiente] float NULL,
    [SubTotal] float NOT NULL,
    [IVA] float NULL,
    [CantAbono] float NULL,
    [Estado] bit NULL,
    CONSTRAINT [PK__Abono__A4693DA7C3FC2DB8] PRIMARY KEY ([IdAbono]),
    CONSTRAINT [FK__Abono__IdReserva__76969D2E] FOREIGN KEY ([IdReserva]) REFERENCES [Reservas] ([IdReserva])
);
GO

CREATE TABLE [DetalleReservaServicio] (
    [IdDetalleReservaServicio] int NOT NULL IDENTITY,
    [IdServicio] int NULL,
    [IdReserva] int NULL,
    [Cantidad] int NULL,
    [Costo] float NULL,
    CONSTRAINT [PK__DetalleR__D3D91A5A2212465C] PRIMARY KEY ([IdDetalleReservaServicio]),
    CONSTRAINT [FK__DetalleRe__IdRes__6FE99F9F] FOREIGN KEY ([IdReserva]) REFERENCES [Reservas] ([IdReserva]),
    CONSTRAINT [FK__DetalleRe__IdSer__6EF57B66] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO

CREATE TABLE [DetalleReservaPaquete] (
    [DetalleReservaPaquete] int NOT NULL IDENTITY,
    [IdPaquete] int NULL,
    [IdReserva] int NULL,
    [Cantidad] int NULL,
    [Costo] float NULL,
    CONSTRAINT [PK__DetalleR__2E8BFF251DF99217] PRIMARY KEY ([DetalleReservaPaquete]),
    CONSTRAINT [FK__DetalleRe__IdPaq__72C60C4A] FOREIGN KEY ([IdPaquete]) REFERENCES [Paquetes] ([IdPaquete]),
    CONSTRAINT [FK__DetalleRe__IdRes__73BA3083] FOREIGN KEY ([IdReserva]) REFERENCES [Reservas] ([IdReserva])
);
GO

CREATE TABLE [ImagenPaquete] (
    [IdImagenPaque] int NOT NULL IDENTITY,
    [IdImagen] int NULL,
    [IdPaquete] int NULL,
    CONSTRAINT [PK__ImagenPa__AD53DF9486923A26] PRIMARY KEY ([IdImagenPaque]),
    CONSTRAINT [FK__ImagenPaq__IdIma__5812160E] FOREIGN KEY ([IdImagen]) REFERENCES [Imagenes] ([IdImagen]),
    CONSTRAINT [FK__ImagenPaq__IdPaq__59063A47] FOREIGN KEY ([IdPaquete]) REFERENCES [Paquetes] ([IdPaquete])
);
GO

CREATE TABLE [PaqueteServicio] (
    [IdPaqueteServicio] int NOT NULL IDENTITY,
    [IdPaquete] int NULL,
    [IdServicio] int NULL,
    [Costo] float NULL,
    CONSTRAINT [PK__PaqueteS__DE5C2BC2AAE5362D] PRIMARY KEY ([IdPaqueteServicio]),
    CONSTRAINT [FK__PaqueteSe__IdPaq__5BE2A6F2] FOREIGN KEY ([IdPaquete]) REFERENCES [Paquetes] ([IdPaquete]),
    CONSTRAINT [FK__PaqueteSe__IdSer__5CD6CB2B] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO

CREATE TABLE [ImagenAbono] (
    [IdImagenAbono] int NOT NULL IDENTITY,
    [IdImagen] int NULL,
    [IdAbono] int NULL,
    CONSTRAINT [PK__ImagenAb__A40FAE539D9B414D] PRIMARY KEY ([IdImagenAbono]),
    CONSTRAINT [FK__ImagenAbo__IdAbo__7A672E12] FOREIGN KEY ([IdAbono]) REFERENCES [Abono] ([IdAbono]),
    CONSTRAINT [FK__ImagenAbo__IdIma__797309D9] FOREIGN KEY ([IdImagen]) REFERENCES [Imagenes] ([IdImagen])
);
GO

CREATE INDEX [IX_Abono_IdReserva] ON [Abono] ([IdReserva]);
GO

CREATE INDEX [IX_Clientes_IdRol] ON [Clientes] ([IdRol]);
GO

CREATE INDEX [IX_Clientes_IdTipoDocumento] ON [Clientes] ([IdTipoDocumento]);
GO

CREATE INDEX [IX_DetalleReservaPaquete_IdPaquete] ON [DetalleReservaPaquete] ([IdPaquete]);
GO

CREATE INDEX [IX_DetalleReservaPaquete_IdReserva] ON [DetalleReservaPaquete] ([IdReserva]);
GO

CREATE INDEX [IX_DetalleReservaServicio_IdReserva] ON [DetalleReservaServicio] ([IdReserva]);
GO

CREATE INDEX [IX_DetalleReservaServicio_IdServicio] ON [DetalleReservaServicio] ([IdServicio]);
GO

CREATE INDEX [IX_Habitaciones_IdTipoHabitacion] ON [Habitaciones] ([IdTipoHabitacion]);
GO

CREATE INDEX [IX_ImagenAbono_IdAbono] ON [ImagenAbono] ([IdAbono]);
GO

CREATE INDEX [IX_ImagenAbono_IdImagen] ON [ImagenAbono] ([IdImagen]);
GO

CREATE INDEX [IX_ImagenHabitacion_IdHabitacion] ON [ImagenHabitacion] ([IdHabitacion]);
GO

CREATE INDEX [IX_ImagenHabitacion_IdImagen] ON [ImagenHabitacion] ([IdImagen]);
GO

CREATE INDEX [IX_ImagenPaquete_IdImagen] ON [ImagenPaquete] ([IdImagen]);
GO

CREATE INDEX [IX_ImagenPaquete_IdPaquete] ON [ImagenPaquete] ([IdPaquete]);
GO

CREATE INDEX [IX_ImagenServicio_IdImagen] ON [ImagenServicio] ([IdImagen]);
GO

CREATE INDEX [IX_ImagenServicio_IdServicio] ON [ImagenServicio] ([IdServicio]);
GO

CREATE INDEX [IX_Paquetes_IdHabitacion] ON [Paquetes] ([IdHabitacion]);
GO

CREATE INDEX [IX_PaqueteServicio_IdPaquete] ON [PaqueteServicio] ([IdPaquete]);
GO

CREATE INDEX [IX_PaqueteServicio_IdServicio] ON [PaqueteServicio] ([IdServicio]);
GO

CREATE INDEX [IX_PermisosRoles_IdPermiso] ON [PermisosRoles] ([IdPermiso]);
GO

CREATE INDEX [IX_PermisosRoles_IdRol] ON [PermisosRoles] ([IdRol]);
GO

CREATE INDEX [IX_Reservas_IdEstadoReserva] ON [Reservas] ([IdEstadoReserva]);
GO

CREATE INDEX [IX_Reservas_MetodoPago] ON [Reservas] ([MetodoPago]);
GO

CREATE INDEX [IX_Reservas_NroDocumentoCliente] ON [Reservas] ([NroDocumentoCliente]);
GO

CREATE INDEX [IX_Reservas_NroDocumentoUsuario] ON [Reservas] ([NroDocumentoUsuario]);
GO

CREATE INDEX [IX_Servicios_IdTipoServicio] ON [Servicios] ([IdTipoServicio]);
GO

CREATE INDEX [IX_Usuarios_IdRol] ON [Usuarios] ([IdRol]);

