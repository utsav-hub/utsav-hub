from django.urls import path

from .import views


urlpatterns = [
    path('company/<int:id>',views.company,name="company"),
    path('new',views.new,name="new"),
]