from django.shortcuts import render ,get_object_or_404
from django.forms import modelform_factory
from django.http import HttpResponse
from Masters.models import Company


def welcome(request):
    return render(request,"Masters/home.html",
    {"companies":Company.objects.all()})

def company(request,id):
    comp = Company.objects.get(pk=id)
    return render(request,"Masters/details.html",
    {"company":comp})

CompanyForm = modelform_factory(Company,exclude=[])

def new(request):
    if request.method == "POST":
       form = CompanyForm(request.POST)
       if form.is_valid():
            form.save()
            return redirect("welcome")
    else:      
        form = CompanyForm()
    return render(request,"Masters/new.html",{"form":form})