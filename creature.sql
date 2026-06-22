-- таблица ролей
create table role (
    id_role serial primary key,
    name varchar(255) not null,
    description text
);

-- таблица пользователей
create table "user" (
    id_user serial primary key,
    id_role int not null,
    login varchar(255) not null unique,
    password varchar(255) not null,
    foreign key (id_role) references role(id_role)
);

-- таблица сотрудников
create table employee (
    id_employee serial primary key,
    id_user int not null unique,
    first_name varchar(255) not null,
    last_name varchar(255) not null,
    middle_name varchar(255),
    birth_date date,
    id_boss int default null,
    is_active boolean default true not null,
    foreign key (id_user) references "user"(id_user),
    foreign key (id_boss) references employee(id_employee)
);

-- таблица статусов
create table status (
    id_status serial primary key,
    name varchar(255) not null,
    description text
);

-- таблица задач
create table task (
    id_task serial primary key,
    id_status int not null,
    id_creator int not null,
    name varchar(255) not null,
    description text,
    creation_date date not null,
    completion_date date,
    foreign key (id_status) references status(id_status),
    foreign key (id_creator) references employee(id_employee)
);

-- таблица исполнителей
create table executor (
    id_executor serial primary key,
    id_task int not null,
    id_employee int not null,
    is_active boolean default false,
    comment text,
    change_date date,
    foreign key (id_task) references task(id_task),
    foreign key (id_employee) references employee(id_employee)
);

-- таблица комментариев
create table comment (
    id_comment serial primary key,
    id_task int not null,
    id_user int not null,
    text text not null,
    add_date date not null,
    foreign key (id_task) references task(id_task),
    foreign key (id_user) references "user"(id_user)
);

-- таблица файлов
create table file (
    id_file serial primary key,
    id_task int not null,
    name varchar(255) not null,
    add_date date not null,
    foreign key (id_task) references task(id_task)
);




