--datos quemados para ejemplificar que las vistas y el controlador acceden correctamente a la informacion de otras tablas

use master

go

use GVGlamping

go

insert into Servicios (IdTipoServicio,NomServicio,costo,Estado) values
(2,'Escalada',12000,1),
(2,'Rafting',144000,1)

go

insert into TipoHabitaciones (NombreTipoHabitacion,NumeroPersonas,Estado) values
('king Size',2,1),
('Presidencial',4,1)

go

insert into Paquetes(NomPaquete,Costo,IdHabitacion,Estado,Descripcion) values
('Comidas',290000,1,1,'Comidas nutritivas'),
('Aventura',311000,3,1,'Para gente aventurera')

go

insert into PaqueteServicio(IdPaquete,IdServicio,Costo) values
(1,1,30000),
(1,2,30000),
(1,3,30000),
(2,4,100000),
(2,5,12000),
(2,6,144000)

go

insert into TipoDocumento(NomTipoDocumento) values
('Pasaporte')

go

insert into Clientes(IdTipoDocumento,Nombres,Apellidos,Celular,Correo,Contrasena,Estado,IdRol) values
(3,'Tom','Bombadil',3013183126,'withoutFather@firstage.com','goldenberry',1,2),
(1,'Bilbo','Bolson',3013183117,'bolsonCerrado1@comarca.com','HateTrolls',1,2),
(2,'Frodo','Bolson',3013183129,'bolsonCerrado2@comarca.com','ImissBilbo',1,2),
(3,'Thorin','II',3013183126,'OakenShield@sonsofdurin.com','dwarfFreedom',1,2),
(2,'Osamu','Dazai',3013183126,'nolongerhuman@portmafia.com','goldenberry',1,2)