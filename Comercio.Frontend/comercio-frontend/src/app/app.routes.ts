import { Routes } from '@angular/router';
import { ProductoListComponent } from './features/productos/components/producto-list.component/producto-list.component';
import { ProductoDetalle } from './features/productos/components/producto-detalle/producto-detalle';
import { ProductoCrear } from './features/productos/components/producto-crear/producto-crear';
import { Component } from '@angular/core';
import { ProductoModificar } from './features/productos/components/producto-modificar/producto-modificar';
import { LoginComponent } from './features/auth/components/login/login-component/login-component';
import { Register } from './features/auth/components/register/register';
import { authGuard } from './core/guards/auth-guard';
import { AccesoDenegado } from './features/auth/components/acceso-denegado/acceso-denegado';
import { CarritoComponent } from './features/carrito/components/carrito-component/carrito-component';
import { authenticatedGuardGuard } from './core/guards/authenticated-guard-guard';

export const routes: Routes = [
    {path:'', redirectTo: 'productos', pathMatch: 'full'},
    {path:'login', component: LoginComponent},
    {path:'register', component: Register},
    {path:'acceso-denegado', component: AccesoDenegado},
    {path:'carritos/items', component: CarritoComponent, canActivate: [authenticatedGuardGuard]},
    {path:'productos', component: ProductoListComponent},
    {path:'productos/crear', component: ProductoCrear, canActivate: [authGuard]},
    {path:'productos/:id/editar', component: ProductoModificar, canActivate: [authGuard]},
    {path:'productos/:id', component: ProductoDetalle},
    {path: '**', redirectTo: ''}
];
