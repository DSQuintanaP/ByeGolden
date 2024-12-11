--datos quemados para ejemplificar que las vistas y el controlador acceden correctamente a la informacion de otras tablas

use master

go

use ByeGolden

go

insert into MetodoPago(NomMetodoPago)
values ('Efectivo'),
       ('Tarjeta Credito'),
	   ('Tarjeta Debito'),
	   ('DataCuerpo')

GO

insert into EstadosReserva(NombreEstadoReserva)
values ('Reservado'),
       ('Por Confirmar'),
	   ('Confirmado'),
	   ('En Ejecución'),
	   ('Anulado'),
	   ('Finalizado')

go

insert into TipoServicios(NombreTipoServicio) values
('Actividades al aire Libre'),
('Actividades Creativas')

go

insert into Servicios(IdTipoServicio,NomServicio,Costo,Descripcion,Estado) values
(1,'Cabalgata',60000,'Paseo en Caballo',1),
(1,'Caminata',30000,'Paseo por la reserva forestal',1),
(2,'Manualidades',45000,'Taller de artes plasticas',1),
(1,'Rafting',80000,'Paseo por el embalse ',1)

go

insert into TipoHabitaciones(NomTipoHabitacion,NumeroPersonas,Estado)
values ('Simple',2,1),
       ('Doble',4,1),
	   ('Familiar',8,1),
	   ('Suite',2,1)

GO

insert into Habitaciones(IdTipoHabitacion,Nombre,Estado,Descripcion,Costo)
values (1,'Habitacion Simple',1,'Habitacion para dos personas',200000),
       (2,'Habitacion Doble',1,'Habitacion para cuatro personas',300000),
	   (2,'Habitacion Familiar',1,'Habitacion para ocho personas',400000),
	   (2,'Habitacion Premium',1,'Habitacion para dos personas',500000)

go

insert into Paquetes(NomPaquete,Costo,IdHabitacion,Estado,Descripcion) values
('Aventura',450000,2,1,'Actividades Recreativas Grupales'),
('Taller Artisctico ',440000,3,1,'Actividades de Artes plasticas')

go

insert into PaqueteServicio(IdPaquete,IdServicio,Costo) values
(3,1,60000),
(3,2,30000),
(3,4,80000),
(4,3,45000)

go

insert into Roles(NomRol,Estado)
values ('Administrador',1),
       ('Cliente',1)

go

insert into TipoDocumento(NomTipoDcumento) values 
	('C.C'),
	('T.I'),
	('Pasaporte')

go

insert into Clientes(NroDocumento,IdTipoDocumento,Nombres,Apellidos,Celular,Correo,Contrasena,Confirmado,Restablecer,Estado,IdRol) values
('123',3,'Tom','Bombadil',3013183126,'withoutFather@firstage.com','goldenberry',1,0,1,2),
('456',1,'Bilbo','Bolson',3013183117,'bolsonCerrado1@comarca.com','HateTrolls',1,0,1,2),
('789',2,'Frodo','Bolson',3013183129,'bolsonCerrado2@comarca.com','ImissBilbo',1,0,1,2),
('153',3,'Thorin','II',3013183126,'OakenShield@sonsofdurin.com','dwarfFreedom',1,0,1,2),
('159',2,'Osamu','Dazai',3013183126,'nolongerhuman@portmafia.com','ILoveChuuyaNakahara',1,0,1,2)