import { Role } from "./role";

export interface RegisterCredentials {
    email: string;
    password: string;
    role: Role;
}