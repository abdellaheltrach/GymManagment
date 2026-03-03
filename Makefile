.PHONY: up down down-volume logs-app migrate test

up:
	docker compose up -d

down:
	docker compose down

down-volume:
	docker compose down -v

logs-app:
	docker compose logs -f web

migrate:
	dotnet ef database update --project GymManagement.Infrastructure --startup-project GymManagement

test:
	dotnet test GymManagement.sln
