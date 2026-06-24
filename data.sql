-- Данные таблицы роль
INSERT INTO accounting_task."role" ("name",description) VALUES
	 ('администратор','полные права доступа'),
	 ('руководитель','управление задачами подчиненных'),
	 ('сотрудник','базовые права сотрудника');

-- Данные для таблицы пользователь
INSERT INTO accounting_task."user" (id_role,login,"password") VALUES
	 (1,'admin','$10$26JccmsYCTP7LyNNBnwYQe84AB273a2MtIv1i5bIJ/xzWuGwKu1Tu');

-- Данные для таблицы сотрудники
INSERT INTO accounting_task.employee (id_user,first_name,last_name,middle_name,id_boss,is_active) VALUES
	 (1,'admin','adminov','adminovich',NULL,true);

-- Данные для таблицы статус
INSERT INTO accounting_task.status ("name",description) VALUES
	 ('Новая','задача только создана'),
	 ('В ожидании','ожидает назначения или старта'),
	 ('В работе','задача выполняется'),
	 ('Решена','задача завершена'),
	 ('Делегирование','задача передается другому исполнителю');



