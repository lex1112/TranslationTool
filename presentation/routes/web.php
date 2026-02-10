<?php

use App\Http\Controllers\LoginController;
use App\Http\Controllers\TranslationController;
use Illuminate\Support\Facades\Route;

Route::get('/', [LoginController::class, 'showLoginForm'])->name('login');
Route::get('/login', [LoginController::class, 'showLoginForm'])->name('login');
Route::post('/login', [LoginController::class, 'login']);
Route::get('/login/callback', [LoginController::class, 'callback'])->name('login.callback');


Route::middleware(['auth.oidc'])->group(function () {
    Route::get('/translation', [TranslationController::class, 'index'])->name('index');
    Route::get('/translation/edit/{sid}', [TranslationController::class, 'edit'])->name('edit');
    Route::post('/translation/edit/{sid}', [TranslationController::class, 'update'])->name('update');
    Route::delete('/translation/delete/{sid}', [TranslationController::class, 'destroy'])->name('delete');
    Route::get('/translation/add', [TranslationController::class, 'create'])->name('translations.create');
    Route::post('/translations/add', [TranslationController::class, 'store'])->name('translations.store');
});
